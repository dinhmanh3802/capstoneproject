using AutoMapper;
using SCCMS.Domain.DTOs.UserDtos;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Utility;
using SCCMS.API.Services;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SCCMS.Domain.DTOs.Auth.ForgotPassword;
using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using OfficeOpenXml.Style;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;

namespace SCCMS.Domain.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly Dictionary<string, string> _roleNameMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { "Thư ký", "Secretary" },
    { "Huynh trưởng", "Leader" },
    { "Nhân viên", "Staff" },
    { "Trưởng ban", "Supervisor" }
};
        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // Phương thức kiểm tra họ tên không chứa ký tự đặc biệt
        private bool ContainsSpecialCharacters(string input)
        {
            // Chỉ cho phép chữ cái và khoảng trắng
            return !Regex.IsMatch(input, @"^[\p{L}\s]+$");
        }

        // Phương thức kiểm tra Email hợp lệ
        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Phương thức kiểm tra số điện thoại hợp lệ
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            return Regex.IsMatch(phoneNumber, @"^\d{10,11}$");
        }

        // Phương thức tạo Username dựa trên FullName và số ngẫu nhiên
        private async Task<string> GenerateUsernameAsync(string fullName)
        {
            // Loại bỏ dấu trước khi xử lý để tránh việc có ký tự dấu trong username
            fullName = RemoveDiacritics(fullName.Trim());

            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Nếu chỉ có một phần trong FullName, sử dụng chính phần đó
            string baseUsername;
            if (parts.Length == 1)
            {
                baseUsername = parts[0].ToLower(); // Chỉ dùng tên duy nhất
            }
            else
            {
                var lastName = parts[^1].ToLower(); // Lấy phần cuối cùng làm tên (ví dụ: Mạnh -> manh)
                var initials = string.Join("", parts.Take(parts.Length - 1).Select(p => p[0].ToString().ToLower())); // Lấy chữ cái đầu của họ và tên đệm

                baseUsername = $"{lastName}{initials}"; // Ghép tên cuối và chữ cái đầu từ các phần tên đệm và họ
            }

            // Tìm tất cả các username bắt đầu bằng baseUsername
            var existingUsernames = await _unitOfWork.User.FindAsync(u => u.UserName.StartsWith(baseUsername));

            // Lấy các số đuôi hiện tại
            var suffixNumbers = existingUsernames
                .Select(u =>
                {
                    var suffix = u.UserName.Substring(baseUsername.Length);
                    if (int.TryParse(suffix, out int num))
                        return num;
                    return 0;
                })
                .Where(n => n > 0)
                .ToList();

            // Xác định số tiếp theo
            int nextNumber = suffixNumbers.Any() ? suffixNumbers.Max() + 1 : 1;

            var newUsername = $"{baseUsername}{nextNumber}";

            return newUsername;
        }

        // Phương thức tạo Username cho BulkCreateUsersAsync
        private async Task<string> GenerateUsernameForBulkAsync(string fullName, List<string> existingUsernamesInFile)
        {
            fullName = RemoveDiacritics(fullName.Trim());
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string baseUsername;
            if (parts.Length == 1)
            {
                baseUsername = parts[0].ToLower();
            }
            else
            {
                var lastName = parts[^1].ToLower();
                var initials = string.Join("", parts.Take(parts.Length - 1).Select(p => p[0].ToString().ToLower()));
                baseUsername = $"{lastName}{initials}";
            }

            var existingUsernamesInDb = await _unitOfWork.User.FindAsync(u => u.UserName.StartsWith(baseUsername));

            var allExistingUsernames = existingUsernamesInDb.Select(u => u.UserName).Concat(existingUsernamesInFile).ToList();

            var suffixNumbers = allExistingUsernames
                .Where(u => u.StartsWith(baseUsername))
                .Select(u =>
                {
                    var suffix = u.Substring(baseUsername.Length);
                    if (int.TryParse(suffix, out int num))
                        return num;
                    return 0;
                })
                .Where(n => n > 0)
                .ToList();

            int nextNumber = suffixNumbers.Any() ? suffixNumbers.Max() + 1 : 1;

            var newUsername = $"{baseUsername}{nextNumber}";
            existingUsernamesInFile.Add(newUsername);

            return newUsername;
        }

        // Phương thức loại bỏ dấu tiếng Việt
        private string RemoveDiacritics(string text)
        {
            // Thay thế các ký tự đặc biệt 'đ' và 'Đ' trước khi loại bỏ dấu
            text = text.Replace('Đ', 'D').Replace('đ', 'd');

            // Chuẩn hóa chuỗi để loại bỏ các dấu còn lại
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        // Phương thức tạo Password ngẫu nhiên
        private string GeneratePassword(int length = 8)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?";
            Random random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                                        .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Phương thức ValidatePassword
        private string ValidatePassword(string password)
        {
            const int minLength = 8;
            var hasUpperCase = password.Any(char.IsUpper);
            var hasLowerCase = password.Any(char.IsLower);
            var hasNumbers = password.Any(char.IsDigit);
            var hasSpecialChars = password.Any(ch => "!@#$%^&*(),.?\":{}|<>".Contains(ch));

            if (password.Length < minLength)
            {
                return "Mật khẩu phải có ít nhất 8 ký tự.";
            }
            if (!hasUpperCase)
            {
                return "Mật khẩu phải có ít nhất 1 chữ hoa.";
            }
            if (!hasLowerCase)
            {
                return "Mật khẩu phải có ít nhất 1 chữ thường.";
            }
            if (!hasNumbers)
            {
                return "Mật khẩu phải có ít nhất 1 số.";
            }
            if (!hasSpecialChars)
            {
                return "Mật khẩu phải có ít nhất 1 ký tự đặc biệt.";
            }
            return string.Empty;
        }


        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(
    string? name = null, string? email = null, string? phoneNumber = null,
    UserStatus? status = null,
    Gender? gender = null,
    int? roleId = null,
    DateTime? startDate = null,
    DateTime? endDate = null)
        {
            var users = await _unitOfWork.User.FindAsync(u =>
                (string.IsNullOrEmpty(name) || EF.Functions.Collate(u.FullName, "Latin1_General_CI_AI").Contains(name)) &&
                (string.IsNullOrEmpty(email) || EF.Functions.Collate(u.Email, "Latin1_General_CI_AI").Contains(email)) &&
                (string.IsNullOrEmpty(phoneNumber) || u.PhoneNumber.Contains(phoneNumber)) && // Số điện thoại thường không có dấu
                (status == null || u.Status == status) &&
                (gender == null || u.Gender == gender) &&
                (roleId == null || u.RoleId == roleId) &&
                (!startDate.HasValue || u.DateOfBirth >= startDate.Value) &&
                (!endDate.HasValue || u.DateOfBirth <= endDate.Value));

            if (users == null || !users.Any())
            {
                return new List<UserDto>();
            }

            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
            return userDtos;
        }


        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.User.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _unitOfWork.User.GetAsync(u => u.UserName.ToLower() == username.ToLower());
            if (user == null)
            {
                return null;
            }
            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }

        public async Task CreateUserAsync(UserCreateDto userCreateDto)
        {
            var errorMessages = new List<string>();

            // Kiểm tra Họ tên không được để trống
            if (string.IsNullOrWhiteSpace(userCreateDto.FullName))
            {
                errorMessages.Add("Họ tên không được để trống.");
            }
            if (userCreateDto.FullName.Length > 100)
            {
                errorMessages.Add("Họ tên không được vượt quá 100 ký tự.");
            }
            else if (ContainsSpecialCharacters(userCreateDto.FullName))
            {
                errorMessages.Add("Họ tên không được chứa ký tự đặc biệt.");
            }

            // Kiểm tra Địa chỉ không được để trống
            if (string.IsNullOrWhiteSpace(userCreateDto.Address))
            {
                errorMessages.Add("Địa chỉ không được để trống.");
            }
            else if (userCreateDto.Address.Length > 300)
            {
                errorMessages.Add("Địa chỉ không được vượt quá 300 ký tự.");
            }
            // Kiểm tra số điện thoại
            if (string.IsNullOrWhiteSpace(userCreateDto.PhoneNumber))
            {
                errorMessages.Add("Số điện thoại không được để trống.");
            }
            else if (!IsValidPhoneNumber(userCreateDto.PhoneNumber))
            {
                errorMessages.Add("Số điện thoại sai định dạng.");
            }

            // Kiểm tra ngày sinh
            if (!userCreateDto.DateOfBirth.HasValue)
            {
                errorMessages.Add("Ngày sinh không được để trống.");
            }
            else if (userCreateDto.DateOfBirth.Value >= DateTime.Now)
            {
                errorMessages.Add("Ngày sinh phải nhỏ hơn ngày hiện tại.");
            }

            // Kiểm tra NationalId
            if (string.IsNullOrWhiteSpace(userCreateDto.NationalId))
            {
                errorMessages.Add("Mã định danh không được để trống.");
            }
            else if (!Regex.IsMatch(userCreateDto.NationalId, @"^\d{9}$|^\d{12}$"))
            {
                errorMessages.Add("Mã định danh phải là 9 hoặc 12 số.");
            }


            if (errorMessages.Any())
            {
                throw new ArgumentException(string.Join(" ", errorMessages));
            }

            // Kiểm tra nếu RoleId có tồn tại trong hệ thống hay không
            var role = await _unitOfWork.Role.GetByIdAsync(userCreateDto.RoleId);
            if (role == null)
            {
                throw new ArgumentException("RoleId không tồn tại.");
            }

            // Tạo Username và Password ngẫu nhiên
            var generatedUsername = await GenerateUsernameAsync(userCreateDto.FullName);
            var generatedPassword = GeneratePassword();

            // Chuyển đổi DTO thành Entity và thiết lập Username, PasswordHash
            var userEntity = _mapper.Map<User>(userCreateDto);
            userEntity.UserName = generatedUsername;
            userEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(generatedPassword);
            userEntity.Status = UserStatus.Active; // Đặt trạng thái là Active
            userEntity.UpdatedBy = userCreateDto.CreatedBy;
            userEntity.DateCreated = DateTime.Now;
            userEntity.DateModified = DateTime.Now;

            // Kiểm tra trùng lặp trước khi thêm vào cơ sở dữ liệu
            // Kiểm tra Email
            var existingUserByEmail = await _unitOfWork.User.FindAsync(u => u.Email.ToLower() == userEntity.Email.ToLower());
            if (existingUserByEmail.Any())
            {
                errorMessages.Add("Email đã tồn tại.");
            }

            // Kiểm tra Số điện thoại
            var existingUserByPhone = await _unitOfWork.User.FindAsync(u => u.PhoneNumber == userEntity.PhoneNumber);
            if (existingUserByPhone.Any())
            {
                errorMessages.Add("Số điện thoại đã tồn tại.");
            }

            // Kiểm tra Mã định danh
            var existingUserByNationalId = await _unitOfWork.User.FindAsync(u => u.NationalId == userEntity.NationalId);
            if (existingUserByNationalId.Any())
            {
                errorMessages.Add("Mã định danh đã tồn tại.");
            }

            if (errorMessages.Any())
            {
                throw new ArgumentException(string.Join(" ", errorMessages));
            }

            // Tạo nội dung email
            var parameters = new Dictionary<string, string>
            {
                { "FullName", userEntity.FullName },
                { "UserName", userEntity.UserName },
                { "Password", generatedPassword }
            };

            try
            {
                // Gửi email trước
                await _emailService.SendEmailAsync(userEntity.Email, "AccountCreation", parameters);

                // Thêm người dùng vào cơ sở dữ liệu nếu email gửi thành công
                await _unitOfWork.User.AddAsync(userEntity);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (DbUpdateException ex)
            {
                // Xử lý các ngoại lệ khác nếu cần thiết
                throw;
            }
        }

        public async Task UpdateUserAsync(int userId, UserUpdateDto userUpdateDto)
        {
            var errorMessages = new List<string>();

            var existingUser = await _unitOfWork.User.GetByIdAsync(userId);
            if (existingUser == null)
            {
                throw new ArgumentException("Người dùng không tồn tại.");
            }

            // Kiểm tra Họ tên không được để trống
            if (string.IsNullOrWhiteSpace(userUpdateDto.FullName))
            {
                errorMessages.Add("Họ tên không được để trống.");
            }
            else if (userUpdateDto.FullName.Length > 100)
            {
                errorMessages.Add("Họ tên không được vượt quá 100 ký tự.");
            }
            else if (ContainsSpecialCharacters(userUpdateDto.FullName))
            {
                errorMessages.Add("Tên không được chứa ký tự đặc biệt.");
            }

            // Kiểm tra Địa chỉ không được để trống
            if (string.IsNullOrWhiteSpace(userUpdateDto.Address))
            {
                errorMessages.Add("Địa chỉ không được để trống.");
            }
            else if (userUpdateDto.Address.Length > 300)
            {
                errorMessages.Add("Địa chỉ không được vượt quá 300 ký tự.");
            }
            // Kiểm tra số điện thoại
            if (string.IsNullOrWhiteSpace(userUpdateDto.PhoneNumber))
            {
                errorMessages.Add("Số điện thoại không được để trống.");
            }
            else if (!IsValidPhoneNumber(userUpdateDto.PhoneNumber))
            {
                errorMessages.Add("Số điện thoại sai định dạng.");
            }

            // Kiểm tra ngày sinh
            if (!userUpdateDto.DateOfBirth.HasValue)
            {
                errorMessages.Add("Ngày sinh không được để trống.");
            }
            else if (userUpdateDto.DateOfBirth.Value >= DateTime.Now)
            {
                errorMessages.Add("Ngày sinh phải nhỏ hơn ngày hiện tại.");
            }

            // Kiểm tra NationalId
            if (string.IsNullOrWhiteSpace(userUpdateDto.NationalId))
            {
                errorMessages.Add("Mã định danh không được để trống.");
            }
            else if (!Regex.IsMatch(userUpdateDto.NationalId, @"^\d{9}$|^\d{12}$"))
            {
                errorMessages.Add("Mã định danh phải là 9 hoặc 12 số.");
            }

            // Kiểm tra xem email có trùng với người dùng khác không
            if (!string.IsNullOrWhiteSpace(userUpdateDto.Email))
            {
                var userWithSameEmail = await _unitOfWork.User.GetAsync(u => u.Email.ToLower() == userUpdateDto.Email.ToLower() && u.Id != userId);
                if (userWithSameEmail != null)
                {
                    errorMessages.Add("Email đã tồn tại.");
                }
            }

            // Kiểm tra xem số điện thoại có trùng với người dùng khác không
            if (!string.IsNullOrWhiteSpace(userUpdateDto.PhoneNumber))
            {
                var userWithSamePhoneNumber = await _unitOfWork.User.GetAsync(u => u.PhoneNumber == userUpdateDto.PhoneNumber && u.Id != userId);
                if (userWithSamePhoneNumber != null)
                {
                    errorMessages.Add("Số điện thoại đã tồn tại.");
                }
            }

            // Kiểm tra xem National ID có trùng với người dùng khác không
            if (!string.IsNullOrWhiteSpace(userUpdateDto.NationalId))
            {
                var userWithSameNationalId = await _unitOfWork.User.GetAsync(u => u.NationalId == userUpdateDto.NationalId && u.Id != userId);
                if (userWithSameNationalId != null)
                {
                    errorMessages.Add("Mã định danh đã tồn tại.");
                }
            }

            if (errorMessages.Any())
            {
                throw new ArgumentException(string.Join(" ", errorMessages));
            }

            // Map dữ liệu từ DTO sang entity người dùng hiện tại
            _mapper.Map(userUpdateDto, existingUser);

            // Cập nhật thông tin người dùng
            await _unitOfWork.User.UpdateAsync(existingUser);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _unitOfWork.User.GetByIdAsync(id);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại.");
            }
            await _unitOfWork.User.DeleteAsync(user);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _unitOfWork.User.GetAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại.");
            }

            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, user.PasswordHash))
            {
                throw new ArgumentException("Mật khẩu cũ không chính xác.");
            }

            // Validate mật khẩu mới
            var passwordError = ValidatePassword(changePasswordDto.NewPassword);
            if (!string.IsNullOrEmpty(passwordError))
            {
                throw new ArgumentException(passwordError);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            await _unitOfWork.User.UpdateAsync(user);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task ChangeUserStatusAsync(List<int> userIds, UserStatus newStatus)
        {
            var users = await _unitOfWork.User.FindAsync(u => userIds.Contains(u.Id));
            if (users == null || !users.Any())
            {
                throw new ArgumentException("Không tìm thấy người dùng.");
            }

            foreach (var user in users)
            {
                user.Status = newStatus;
            }

            await _unitOfWork.User.UpdateRangeAsync(users);
            await _unitOfWork.SaveChangeAsync();
        }

        private async Task<int> MapRoleNameToRoleIdAsync(string roleName)
        {
            if (!_roleNameMapping.TryGetValue(roleName, out string englishRoleName))
            {
                throw new ArgumentException($"Vai trò '{roleName}' không hợp lệ.");
            }

            // Sử dụng ToLower() để so sánh không phân biệt chữ hoa/thường
            var role = await _unitOfWork.Role.GetAsync(r => r.RoleName.ToLower() == englishRoleName.ToLower());
            if (role == null)
            {
                throw new ArgumentException($"Vai trò '{englishRoleName}' không tồn tại trong hệ thống.");
            }
            return role.Id;
        }





        public async Task<byte[]> GenerateExcelTemplateAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Template");

                // Cập nhật tiêu đề cột, thêm cột "Role"
                worksheet.Cells[1, 1].Value = "Email";
                worksheet.Cells[1, 2].Value = "Họ và tên";
                worksheet.Cells[1, 3].Value = "Số điện thoại ";
                worksheet.Cells[1, 4].Value = "Giới tính"; // Nam/Nữ
                worksheet.Cells[1, 5].Value = "Ngày sinh (ngày/tháng/năm)";
                worksheet.Cells[1, 6].Value = "Địa chỉ";
                worksheet.Cells[1, 7].Value = "Mã định danh";
                worksheet.Cells[1, 8].Value = "Vai trò"; // Thêm cột Role

                using (var range = worksheet.Cells[1, 1, 1, 8]) // Cập nhật phạm vi từ cột 1 đến cột 8
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Thiết lập độ rộng cho các cột
                worksheet.Column(1).Width = 25; // Email
                worksheet.Column(2).Width = 20; // Họ và tên
                worksheet.Column(3).Width = 15; // Số điện thoại
                worksheet.Column(4).Width = 10; // Giới tính
                worksheet.Column(5).Width = 30; // Ngày sinh
                worksheet.Column(6).Width = 30; // Địa chỉ
                worksheet.Column(7).Width = 15; // Mã định danh
                worksheet.Column(8).Width = 20; // Vai trò

                // **Đặt định dạng cột Số điện thoại thành Text để giữ nguyên số 0 ở đầu**
                worksheet.Column(3).Style.Numberformat.Format = "@";

                // Thiết lập Data Validation cho cột Gender (cột 4)
                var genderValidation = worksheet.DataValidations.AddListValidation($"D2:D1048576");
                genderValidation.ShowErrorMessage = true;
                genderValidation.ErrorTitle = "Giới Tính Không Hợp Lệ";
                genderValidation.Error = "Vui lòng chọn giữa 'Nam' hoặc 'Nữ'.";
                genderValidation.Formula.Values.Add("Nam");
                genderValidation.Formula.Values.Add("Nữ");

                // Thiết lập Data Validation cho cột Role (cột 8)
                var roleValidation = worksheet.DataValidations.AddListValidation($"H2:H1048576");
                roleValidation.ShowErrorMessage = true;
                roleValidation.ErrorTitle = "Vai Trò Không Hợp Lệ";
                roleValidation.Error = "Vui lòng chọn một trong các vai trò: Thư ký, Huynh trưởng, Nhân viên, Trưởng ban.";
                roleValidation.Formula.Values.Add("Thư ký");
                roleValidation.Formula.Values.Add("Huynh trưởng");
                roleValidation.Formula.Values.Add("Nhân viên");
                roleValidation.Formula.Values.Add("Trưởng ban");

                return await package.GetAsByteArrayAsync();
            }
        }




        public async Task<BulkCreateUsersResultDto> BulkCreateUsersAsync(IFormFile file)
        {
            var result = new BulkCreateUsersResultDto();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        result.HasErrors = true;
                        result.Errors.Add("Không tìm thấy worksheet trong file Excel.");
                        return result;
                    }

                    var rowCount = worksheet.Dimension.Rows;
                    var columnCount = worksheet.Dimension.Columns;

                    // Cập nhật danh sách tiêu đề cột bao gồm "Role"
                    var expectedHeaders = new List<string> { "Email", "FullName", "PhoneNumber", "Gender", "DateOfBirth", "Address", "NationalId", "Role" };
                    for (int col = 1; col <= expectedHeaders.Count; col++)
                    {
                        var header = worksheet.Cells[1, col].Text.Trim();
                        if (!expectedHeaders[col - 1].Equals(header, StringComparison.OrdinalIgnoreCase))
                        {
                            result.HasErrors = true;
                            result.Errors.Add($"Cột {col} không đúng. Mong đợi: {expectedHeaders[col - 1]}, thực tế: {header}");
                        }
                    }

                    if (result.HasErrors)
                    {
                        return result;
                    }

                    // Kiểm tra xem có ít nhất một bản ghi dữ liệu (ngoài tiêu đề) hay không
                    if (rowCount <= 1)
                    {
                        result.HasErrors = true;
                        result.Errors.Add("Không tìm thấy bản ghi nào trong tệp Excel để tạo người dùng.");
                        return result;
                    }

                    var usersToCreate = new List<(User user, string password, int rowNumber)>();
                    var emailsInFile = new HashSet<string>();
                    var phoneNumbersInFile = new HashSet<string>();
                    var nationalIdsInFile = new HashSet<string>();
                    var existingUsernamesInFile = new List<string>();

                    // Sử dụng Dictionary để lưu lỗi cho mỗi dòng
                    var rowErrorsDictionary = new Dictionary<int, List<string>>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var email = worksheet.Cells[row, 1].Text.Trim();
                        var fullName = worksheet.Cells[row, 2].Text.Trim();
                        var phoneNumber = worksheet.Cells[row, 3].Text.Trim();
                        var genderText = worksheet.Cells[row, 4].Text.Trim();
                        var dateOfBirthText = worksheet.Cells[row, 5].Text.Trim();
                        var address = worksheet.Cells[row, 6].Text.Trim();
                        var nationalId = worksheet.Cells[row, 7].Text.Trim();
                        var roleText = worksheet.Cells[row, 8].Text.Trim(); // Lấy giá trị từ cột Role

                        var rowErrors = new List<string>();

                        // Kiểm tra Email
                        if (string.IsNullOrWhiteSpace(email))
                        {
                            rowErrors.Add("Email là bắt buộc.");
                        }
                        else if (!IsValidEmail(email))
                        {
                            rowErrors.Add("Email không hợp lệ.");
                        }

                        // Kiểm tra FullName
                        if (string.IsNullOrWhiteSpace(fullName))
                        {
                            rowErrors.Add("FullName là bắt buộc.");
                        }
                        else if (ContainsSpecialCharacters(fullName))
                        {
                            rowErrors.Add("Tên không được chứa ký tự đặc biệt.");
                        }

                        // Kiểm tra PhoneNumber
                        if (string.IsNullOrWhiteSpace(phoneNumber))
                        {
                            rowErrors.Add("Số điện thoại là bắt buộc.");
                        }
                        else if (!IsValidPhoneNumber(phoneNumber))
                        {
                            rowErrors.Add("Số điện thoại sai định dạng.");
                        }

                        // Kiểm tra DateOfBirth
                        DateTime? dateOfBirthParsed = null;
                        if (string.IsNullOrWhiteSpace(dateOfBirthText))
                        {
                            rowErrors.Add("Ngày sinh là bắt buộc.");
                        }
                        else if (!DateTime.TryParse(dateOfBirthText, out DateTime parsedDate))
                        {
                            rowErrors.Add("Ngày sinh không hợp lệ.");
                        }
                        else if (parsedDate >= DateTime.Now)
                        {
                            rowErrors.Add("Ngày sinh phải nhỏ hơn ngày hiện tại.");
                        }
                        else
                        {
                            dateOfBirthParsed = parsedDate;
                        }

                        // Kiểm tra Address
                        if (string.IsNullOrWhiteSpace(address))
                        {
                            rowErrors.Add("Address là bắt buộc.");
                        }

                        // Kiểm tra NationalId
                        if (string.IsNullOrWhiteSpace(nationalId))
                        {
                            rowErrors.Add("NationalId là bắt buộc.");
                        }
                        else if (!Regex.IsMatch(nationalId, @"^\d{9}$|^\d{12}$"))
                        {
                            rowErrors.Add("NationalId phải là 9 hoặc 12 số.");
                        }

                        // Kiểm tra Role
                        if (string.IsNullOrWhiteSpace(roleText))
                        {
                            rowErrors.Add("Role là bắt buộc.");
                        }
                        else
                        {
                            var validRoles = new List<string> { "Thư ký", "Huynh trưởng", "Nhân viên", "Trưởng ban" };
                            if (!validRoles.Contains(roleText, StringComparer.OrdinalIgnoreCase))
                            {
                                rowErrors.Add("Role không hợp lệ. Các vai trò hợp lệ: Thư ký, Huynh trưởng, Nhân viên, Trưởng ban.");
                            }
                        }

                        // Kiểm tra trùng lặp trong file
                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            if (emailsInFile.Contains(email.ToLower()))
                            {
                                rowErrors.Add("Email bị trùng trong file.");
                            }
                            else
                            {
                                emailsInFile.Add(email.ToLower());
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(phoneNumber))
                        {
                            if (phoneNumbersInFile.Contains(phoneNumber))
                            {
                                rowErrors.Add("Số điện thoại bị trùng trong file.");
                            }
                            else
                            {
                                phoneNumbersInFile.Add(phoneNumber);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(nationalId))
                        {
                            if (nationalIdsInFile.Contains(nationalId))
                            {
                                rowErrors.Add("NationalId bị trùng trong file.");
                            }
                            else
                            {
                                nationalIdsInFile.Add(nationalId);
                            }
                        }

                        // Kiểm tra và map Gender
                        Gender gender = Gender.Male; // Default
                        if (!string.IsNullOrWhiteSpace(genderText))
                        {
                            if (genderText.Equals("Nam", StringComparison.OrdinalIgnoreCase))
                            {
                                gender = Gender.Male;
                            }
                            else if (genderText.Equals("Nữ", StringComparison.OrdinalIgnoreCase))
                            {
                                gender = Gender.Female;
                            }
                            else
                            {
                                rowErrors.Add("Giới tính không hợp lệ. Vui lòng chọn 'Nam' hoặc 'Nữ'.");
                            }
                        }
                        else
                        {
                            // Nếu Gender không được cung cấp
                            rowErrors.Add("Giới tính là bắt buộc.");
                        }

                        // Nếu có lỗi trong các kiểm tra trên, ghi lại và tiếp tục
                        if (rowErrors.Any())
                        {
                            if (!rowErrorsDictionary.ContainsKey(row))
                            {
                                rowErrorsDictionary[row] = new List<string>();
                            }
                            rowErrorsDictionary[row].AddRange(rowErrors);
                            continue;
                        }

                        // Map Role
                        int mappedRoleId;
                        try
                        {
                            mappedRoleId = await MapRoleNameToRoleIdAsync(roleText);
                        }
                        catch (ArgumentException ex)
                        {
                            rowErrors.Add(ex.Message);
                            if (!rowErrorsDictionary.ContainsKey(row))
                            {
                                rowErrorsDictionary[row] = new List<string>();
                            }
                            rowErrorsDictionary[row].AddRange(rowErrors);
                            continue;
                        }

                        // Generate username and password
                        string username = await GenerateUsernameForBulkAsync(fullName, existingUsernamesInFile);
                        string password = GeneratePassword();

                        // Create User entity
                        var user = new User
                        {
                            Email = email,
                            FullName = fullName,
                            PhoneNumber = phoneNumber,
                            Gender = gender,
                            DateOfBirth = dateOfBirthParsed,
                            Address = address,
                            NationalId = nationalId,
                            RoleId = mappedRoleId,
                            UserName = username,
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                            Status = UserStatus.Active
                        };

                        usersToCreate.Add((user, password, row));
                    }

                    // Kiểm tra nếu không có bản ghi dữ liệu hợp lệ nào để tạo
                    if (!usersToCreate.Any())
                    {
                        // Kiểm tra xem tất cả các dòng dữ liệu đều có lỗi hay không
                        if (rowErrorsDictionary.Any())
                        {
                            result.HasErrors = true;

                            int errorRowCount = 0;
                            foreach (var kvp in rowErrorsDictionary.OrderBy(kvp => kvp.Key))
                            {
                                if (errorRowCount >= 50)
                                {
                                    result.Errors.Add("Đã đạt giới hạn 50 lỗi. Vui lòng kiểm tra file Excel để biết thêm chi tiết.");
                                    break;
                                }

                                var row = kvp.Key;
                                var errorsForRow = kvp.Value.Distinct();

                                // Định dạng lỗi với dòng ngắt và đường gạch ngăn cách bằng HTML
                                var formattedErrors = $"<strong>Dòng {row}:</strong><br/>- {string.Join("<br/>- ", errorsForRow)}<br/><hr/>";
                                result.Errors.Add(formattedErrors);

                                errorRowCount++;
                            }

                            // Nếu tất cả các bản ghi đều có lỗi, không cần thông báo riêng về không có bản ghi hợp lệ
                            return result;
                        }
                        else
                        {
                            // Không có bản ghi hợp lệ và cũng không có lỗi (trường hợp này hiếm)
                            result.HasErrors = true;
                            result.Errors.Add("Không tìm thấy bản ghi hợp lệ để tạo người dùng trong tệp Excel.");
                            return result;
                        }
                    }

                    // Kiểm tra duplicates trong cơ sở dữ liệu
                    var emails = usersToCreate.Select(u => u.user.Email.ToLower()).ToHashSet();
                    var phoneNumbers = usersToCreate.Select(u => u.user.PhoneNumber).Where(p => !string.IsNullOrEmpty(p)).ToHashSet();
                    var nationalIds = usersToCreate.Select(u => u.user.NationalId).ToHashSet();

                    var existingUsers = await _unitOfWork.User.FindAsync(u =>
                        emails.Contains(u.Email.ToLower()) ||
                        phoneNumbers.Contains(u.PhoneNumber) ||
                        nationalIds.Contains(u.NationalId)
                    );

                    foreach (var existingUser in existingUsers)
                    {
                        foreach (var (user, _, row) in usersToCreate)
                        {
                            var errorsForRow = new List<string>();

                            if (user.Email.Equals(existingUser.Email, StringComparison.OrdinalIgnoreCase))
                            {
                                errorsForRow.Add($"Email '{user.Email}' đã tồn tại trong hệ thống.");
                            }
                            if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumber.Equals(existingUser.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                            {
                                errorsForRow.Add($"Số điện thoại '{user.PhoneNumber}' đã tồn tại trong hệ thống.");
                            }
                            if (user.NationalId.Equals(existingUser.NationalId, StringComparison.OrdinalIgnoreCase))
                            {
                                errorsForRow.Add($"NationalId '{user.NationalId}' đã tồn tại trong hệ thống.");
                            }

                            if (errorsForRow.Any())
                            {
                                if (!rowErrorsDictionary.ContainsKey(row))
                                {
                                    rowErrorsDictionary[row] = new List<string>();
                                }
                                rowErrorsDictionary[row].AddRange(errorsForRow);
                            }
                        }
                    }

                    // Tập hợp các lỗi từ dictionary và định dạng
                    if (rowErrorsDictionary.Any())
                    {
                        result.HasErrors = true;

                        int errorRowCountLimit = 50;
                        int errorRowCount = 0;
                        foreach (var kvp in rowErrorsDictionary.OrderBy(kvp => kvp.Key))
                        {
                            if (errorRowCount >= errorRowCountLimit)
                            {
                                result.Errors.Add("Đã đạt giới hạn 50 lỗi. Vui lòng kiểm tra file Excel để biết thêm chi tiết.");
                                break;
                            }

                            var row = kvp.Key;
                            var errorsForRow = kvp.Value.Distinct();

                            // Định dạng lỗi với dòng ngắt và đường gạch ngăn cách bằng HTML
                            var formattedErrors = $"<strong>Dòng {row}:</strong><br/>- {string.Join("<br/>- ", errorsForRow)}<br/><hr/>";
                            result.Errors.Add(formattedErrors);

                            errorRowCount++;
                        }

                        return result;
                    }

                    // Nếu không có lỗi, tiến hành tạo người dùng
                    foreach (var (user, password, _) in usersToCreate)
                    {
                        await _unitOfWork.User.AddAsync(user);
                    }

                    await _unitOfWork.SaveChangeAsync();

                    // Gửi email
                    foreach (var (user, password, _) in usersToCreate)
                    {
                        // Lấy template và gửi email
                        var parameters = new Dictionary<string, string>
                {
                    { "FullName", user.FullName },
                    { "UserName", user.UserName },
                    { "Password", password }
                };

                        try
                        {
                            await _emailService.SendEmailAsync(user.Email, "AccountCreation", parameters);
                        }
                        catch (Exception ex)
                        {
                            // Xử lý lỗi khi gửi email, có thể ghi log hoặc thêm thông báo vào kết quả
                            result.Errors.Add($"Gửi email cho người dùng '{user.Email}' thất bại: {ex.Message}");
                        }
                    }
                }
            }

            return result;
        }






        public async Task ChangeUserRoleAsync(List<int> userIds, int newRoleId)
        {
            if (userIds == null || !userIds.Any())
            {
                throw new ArgumentException("Danh sách người dùng không được để trống.");
            }

            // Kiểm tra vai trò mới có tồn tại không
            var newRole = await _unitOfWork.Role.GetByIdAsync(newRoleId);
            if (newRole == null)
            {
                throw new ArgumentException("Vai trò mới không tồn tại.");
            }

            // Lấy danh sách người dùng cần thay đổi
            var users = await _unitOfWork.User.FindAsync(u => userIds.Contains(u.Id));

            if (users == null || !users.Any())
            {
                throw new ArgumentException("Không tìm thấy người dùng cần thay đổi.");
            }

            // Kiểm tra các vai trò hiện tại của người dùng và loại trừ Admin và Manager nếu cần
            foreach (var user in users)
            {
                if (user.RoleId == SD.RoleId_Admin)
                {
                    throw new ArgumentException($"Không thể thay đổi vai trò của người dùng Quản trị viên (ID: {user.Id}).");
                }
                if (user.RoleId == SD.RoleId_Manager)
                {
                    throw new ArgumentException($"Không thể thay đổi vai trò của người dùng Quản lý (ID: {user.Id}).");
                }
            }

            // Thay đổi vai trò và loại bỏ Supervisor khỏi các nhóm hiện tại nếu cần
            foreach (var user in users)
            {
                var oldRoleId = user.RoleId;
                user.RoleId = newRoleId;

                // Nếu thay đổi từ Supervisor sang khác
                if (oldRoleId == SD.RoleId_Supervisor && newRoleId != SD.RoleId_Supervisor)
                {
                    // Lấy các SupervisorStudentGroup liên quan đến user trong các khóa học đang diễn ra
                    var activeSupervisorAssignments = await _unitOfWork.SupervisorStudentGroup
                        .FindAsync(ssg => ssg.SupervisorId == user.Id &&
                                          (ssg.StudentGroup.Course.EndDate >= DateTime.Now ||
                                           ssg.StudentGroup.Course.Status != CourseStatus.closed));

                    foreach (var ssg in activeSupervisorAssignments)
                    {
                        await _unitOfWork.SupervisorStudentGroup.DeleteAsync(ssg);
                    }
                }
            }

            // Cập nhật người dùng trong cơ sở dữ liệu
            await _unitOfWork.User.UpdateRangeAsync(users);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task ResetPasswordAsync(int userId, UserResetPasswordDto resetPasswordDto)
        {
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại.");
            }

            // Validate mật khẩu mới
            var passwordError = ValidatePassword(resetPasswordDto.NewPassword);
            if (!string.IsNullOrEmpty(passwordError))
            {
                throw new ArgumentException(passwordError);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            await _unitOfWork.User.UpdateAsync(user);
            await _unitOfWork.SaveChangeAsync();
        }
        public async Task<IEnumerable<UserDto>> GetAvailableSupervisorsAsync()
        {
            var today = DateTime.Now.Date;

            // 1. Xác định các khóa tu đang diễn ra
            var ongoingCourses = await _unitOfWork.Course.FindAsync(
                c => c.StartDate.Date <= today &&
                     c.EndDate.Date >= today &&
                     c.Status != CourseStatus.closed &&
                     c.Status != CourseStatus.deleted
            );

            var ongoingCourseIds = ongoingCourses.Select(c => c.Id).ToList();

            // 2. Lấy danh sách Staff đang ACTIVE
            var activeStaff = await _unitOfWork.User.FindAsync(
                u => u.RoleId == SD.RoleId_Staff &&
                     u.Status == UserStatus.Active
            );

            // 3. Lấy các Staff đã tham gia bất kỳ ca trực đêm nào trong các khóa tu đang diễn ra
            var busyStaffIds = await _unitOfWork.NightShiftAssignment.FindAsync(
                nsa => ongoingCourseIds.Contains(nsa.NightShift.CourseId)
            );

            var busyStaffIdList = busyStaffIds.Select(nsa => nsa.UserId).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();

            // 4. Lọc ra các Staff không có trong danh sách busyStaffIdList
            var availableStaff = activeStaff.Where(u => !busyStaffIdList.Contains(u.Id)).ToList();

            // 5. Map từ Entity sang DTO
            var availableStaffDtos = _mapper.Map<IEnumerable<UserDto>>(availableStaff);

            return availableStaffDtos;
        }
    }
}

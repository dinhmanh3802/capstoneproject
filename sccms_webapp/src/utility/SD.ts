//apiConstants
// Chạy local thì dùng cái này
export const SD_BASE_URL = "https://localhost:7160/api"
//export const SD_BASE_URL = "http://localhost:5142/api"

// Deploy lên Azure thì dùng cái này
//export const SD_BASE_URL = "https://sccmsapi.azurewebsites.net/api"

export enum SD_Role {
    ADMIN = 1,
    MANAGER = 2,
    STAFF = 3,
    SECRETARY = 4,
    TEAM_LEADER = 5,
    SUPERVISOR = 6,
}

export enum SD_Role_Name {
    ADMIN = "admin",
    MANAGER = "manager",
    SECRETARY = "secretary",
    STAFF = "staff",
    SUPERVISOR = "supervisor",
    TEAM_LEADER = "teamLeader",
}

export enum SD_Role_Name_VN {
    ADMIN = "Quản trị viên",
    MANAGER = "Quản lý",
    SECRETARY = "Thư ký",
    STAFF = "Nhân viên",
    SUPERVISOR = "Huynh trưởng",
    TEAM_LEADER = "Trưởng ban",
}

export enum SD_UserStatus {
    ACTIVE = 0,
    DEACTIVE = 1,
}

export enum SD_CourseStatus {
    notStarted = 0,
    recruiting = 1,
    inProgress = 2,
    closed = 3,
    deleted = 4,
}

export enum SD_Gender {
    Male = 0,
    Female = 1,
}

export enum SD_ProcessStatus {
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Enrolled = 3,
    Graduated = 4,
    DropOut = 5,
    Deleted = 6,
}

export enum SD_ProcessStatus_Name {
    Pending = "Đang chờ",
    Approved = "Đã duyệt",
    Rejected = "Từ chối",
    WaitingForEnroll = "Chờ nhập học",
    Enrolled = "Nhập học",
    Graduated = "Tốt nghiệp",
    DropOut = "Bỏ học",
}

export enum SD_EmployeeProcessStatus_Name {
    Pending = "Đang chờ",
    Approved = "Đã duyệt",
    Rejected = "Từ chối",
    WaitingForEnroll = "Đang chờ",
    Enrolled = "Đã đến",
    Graduated = "Hoàn thành",
    DropOut = "Rời khoá tu",
}

export enum SD_Status {
    Active = 1,
    Deactive = 0,
}

export enum SD_PostStatus {
    Draft = 0,
    Active = 1,
}

export enum SD_PostType {
    Introduction = 0,
    Activities = 1,
    Announcement = 2,
}

export enum SD_NightShiftAssignmentStatus {
    notStarted = 0,
    completed = 1,
    rejected = 2,
    cancelled = 3,
}

export enum SD_NightShiftAssignmentStatus_Name {
    notStarted = "Chưa bắt đầu",
    completed = "Hoàn thành",
    rejected = "Từ chối",
    cancelled = "Đã hủy",
}

export enum SD_ReportStatus {
    NotYet = 0,
    Attending = 1,
    Attended = 2,
    Late = 3,
    Reopened = 4,
    Read = 5,
}
export enum SD_StudentReportStatus {
    Absent = 0,
    Present = 1,
}

export enum SD_ReportType {
    NightShift = 0,
    DailyReport = 1,
}

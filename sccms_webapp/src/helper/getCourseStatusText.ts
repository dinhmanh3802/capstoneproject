import { SD_CourseStatus } from "../utility/SD"

const getCourseStatusText = (status: SD_CourseStatus): string => {
    switch (status) {
        case SD_CourseStatus.notStarted:
            return "Chưa bắt đầu"
        case SD_CourseStatus.recruiting:
            return "Đang tuyển sinh"
        case SD_CourseStatus.inProgress:
            return "Đang diễn ra"
        case SD_CourseStatus.closed:
            return "Đã kết thúc"
        case SD_CourseStatus.deleted:
            return "Đã xóa"
        default:
            return "Trạng thái không xác định"
    }
}

export { getCourseStatusText }

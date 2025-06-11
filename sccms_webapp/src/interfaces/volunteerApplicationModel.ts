import { SD_ProcessStatus } from "../utility/SD"
import courseModel from "./courseModel"
import { userModel } from "./userModel"
import volunteerModel from "./volunteerModel"

export default interface volunteerApplicationModel {
    id: number
    courseId: number
    course: courseModel // Định nghĩa chi tiết của Course ở đâu đó trong code của bạn
    volunteerId: number
    volunteer: volunteerModel // Định nghĩa chi tiết của Student tương tự như trên
    applicationDate: string
    volunteerCode: string
    status: SD_ProcessStatus
    note: string
    reviewerId?: number
    reviewer?: userModel // Định nghĩa chi tiết của User nếu có
    reviewDate?: string // Hoặc `Date` nếu bạn dùng kiểu Date của JS
    sameNationId?: number
}

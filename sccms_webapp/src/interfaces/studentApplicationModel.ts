import { SD_ProcessStatus } from "../utility/SD"
import courseModel from "./courseModel"
import studentModel from "./studentModel"
import { userModel } from "./userModel"

export default interface studentApplicationModel {
    id: number
    courseId: number
    course: courseModel // Định nghĩa chi tiết của Course ở đâu đó trong code của bạn
    studentId: number
    student: studentModel // Định nghĩa chi tiết của Student tương tự như trên
    applicationDate: string
    studentCode: string
    status: SD_ProcessStatus
    note: string
    reviewerId?: number
    reviewer?: userModel // Định nghĩa chi tiết của User nếu có
    reviewDate?: string // Hoặc `Date` nếu bạn dùng kiểu Date của JS
    sameNationId: number
}

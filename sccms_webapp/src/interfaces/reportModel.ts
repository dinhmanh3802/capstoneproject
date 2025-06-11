import { SD_ReportStatus } from "../utility/SD"
import { nightShiftModel } from "./nightShiftModel"
import { roomModel } from "./roomModel"
import { StudentReportDto } from "./studentReportModel"
import { userModel } from "./userModel"

export interface ReportDto {
    room: roomModel
    id?: number
    reportType?: number
    reportDate?: string
    status?: SD_ReportStatus
    reportContent?: string
    submissionDate?: string
    submissionBy?: number
    submittedByUser?: userModel
    studentGroupId?: number
    courseId?: number
    nightShift: nightShiftModel
    studentGroup?: {
        id: number
        groupName: string
    }
    studentReports?: StudentReportDto[]
    isEditable?: boolean
}

export interface AttendanceSummaryDto {
    date: string
    totalStudents: number
    totalPresent: number
}

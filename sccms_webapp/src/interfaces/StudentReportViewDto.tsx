import { SD_ReportType } from "../utility/SD"

export interface StudentReportViewDto {
    date: string
    status: number
    content: string
    reportType: SD_ReportType
}

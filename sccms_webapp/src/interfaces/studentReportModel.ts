export interface StudentReportDto {
    studentId: number
    studentCode?: string
    studentName: string
    studentImage?: string
    status: number
    comment?: string // Thêm ghi chú nếu có
}

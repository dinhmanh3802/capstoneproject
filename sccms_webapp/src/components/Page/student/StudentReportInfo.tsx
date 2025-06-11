import React from "react"
import { useGetReportsByStudentQuery } from "../../../apis/reportApi"
import { parseISO, format } from "date-fns"

function StudentReportInfo({ studentApplication, courseId }: any) {
    const { data, error, isLoading } = useGetReportsByStudentQuery(
        { courseId: courseId, studentId: studentApplication.studentId },
        {
            skip: !studentApplication || !courseId,
        },
    )
    if (isLoading) return <div>Đang tải...</div>

    const getGroupedReports = () => {
        if (!data?.result || data.result.length === 0) return []

        // Reduce the reports to group by date
        const grouped: { [key: string]: string[] } = data.result.reduce((acc, report) => {
            const dateKey = format(parseISO(report.date), "dd/MM/yyyy")
            if (!acc[dateKey]) {
                acc[dateKey] = []
            }
            if (report.content && report.content.trim() !== "") {
                acc[dateKey].push(report.content)
            }
            return acc
        }, {} as { [key: string]: string[] })

        // Convert the grouped object to an array for rendering, excluding empty comment groups
        return Object.entries(grouped)
            .filter(([date, comments]) => comments.length > 0)
            .map(([date, comments]) => ({
                date,
                comments: comments.join("\n"), // Join comments with line breaks
            }))
    }

    // Retrieve the grouped reports
    const groupedReports = getGroupedReports()
    return (
        <div>
            {error && (
                <div className="alert alert-danger">
                    <p>Không tìm thấy đánh giá</p>
                </div>
            )}

            {/* Display Grouped Reports */}
            {groupedReports.length > 0 ? (
                <div>
                    <table className="table table-bordered">
                        <thead>
                            <tr>
                                <th>Ngày</th>
                                <th>Nhận xét</th>
                            </tr>
                        </thead>
                        <tbody>
                            {groupedReports.map((report, index) => {
                                return (
                                    <tr key={index}>
                                        <td>{report.date}</td>
                                        <td style={{ whiteSpace: "pre-line" }}>{report.comments}</td>
                                    </tr>
                                )
                            })}
                        </tbody>
                    </table>
                </div>
            ) : (
                !isLoading && !error && <p>Không tìm thấy báo cáo nào.</p>
            )}
        </div>
    )
}

export default StudentReportInfo

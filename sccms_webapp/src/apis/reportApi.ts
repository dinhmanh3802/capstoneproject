// src/apis/reportApi.ts

import { createApi } from "@reduxjs/toolkit/query/react"
import customBaseQuery from "./baseQuery"
import { AttendanceSummaryDto, ReportDto, StudentReportDto } from "../interfaces"

export const reportApi = createApi({
    reducerPath: "reportApi",
    baseQuery: customBaseQuery,
    tagTypes: ["Report"],
    endpoints: (builder) => ({
        // Lấy chi tiết báo cáo (dùng chung cho tất cả các vai trò)
        getReport: builder.query({
            query: ({ reportId, reportType, reportDate, roomId, nightShiftId }) => ({
                url: `Report/report`,
                params: {
                    ...(reportId && { reportId }),
                    ...(reportType != null && { reportType }),
                    ...(reportDate && { reportDate }),
                    ...(roomId && { roomId }),
                    ...(nightShiftId && { nightShiftId }),
                },
            }),
            providesTags: ["Report"],
        }),

        // Supervisor: Nộp báo cáo điểm danh
        submitAttendanceReport: builder.mutation<
            void,
            {
                reportId: number
                studentReports: StudentReportDto[]
                reportContent: string
            }
        >({
            query: ({ reportId, studentReports, reportContent }) => ({
                url: `Report/supervisor/report/${reportId}`,
                method: "POST",
                body: { studentReports, reportContent },
            }),
            invalidatesTags: ["Report"],
        }),

        // Supervisor: Yêu cầu mở lại báo cáo
        requestReopenReport: builder.mutation<void, number>({
            query: (reportId) => ({
                url: `Report/request-reopen/${reportId}`,
                method: "POST",
            }),
            invalidatesTags: ["Report"],
        }),

        // Manager: Lấy báo cáo điểm danh theo ngày
        getAttendanceReportsByDate: builder.query<
            { result: ReportDto[] },
            {
                courseId: number
                reportDate?: string
                status?: number
                studentGroupId?: number
            }
        >({
            query: ({ courseId, ...params }) => ({
                url: `Report/attendance-reports/${courseId}`,
                params,
            }),
            providesTags: ["Report"],
        }),

        // Manager: Đánh dấu báo cáo đã đọc
        markReportAsRead: builder.mutation<void, number>({
            query: (reportId) => ({
                url: `Report/manager/mark-as-read/${reportId}`,
                method: "POST",
            }),
            invalidatesTags: ["Report"],
        }),

        // Manager: Mở lại báo cáo
        reopenReport: builder.mutation<void, number>({
            query: (reportId) => ({
                url: `Report/manager/reopen-report/${reportId}`,
                method: "POST",
            }),
            invalidatesTags: ["Report"],
        }),

        // Staff: Nộp báo cáo trực đêm
        submitNightShiftReport: builder.mutation<
            void,
            {
                reportId: number
                studentReports: StudentReportDto[]
                reportContent: string
            }
        >({
            query: ({ reportId, studentReports, reportContent }) => ({
                url: `Report/staff/nightshift-report/${reportId}`,
                method: "POST",
                body: { studentReports, reportContent },
            }),
            invalidatesTags: ["Report"],
        }),

        getReportsByStudent: builder.query({
            query: ({ studentCode, courseId, studentId }) => ({
                url: `Report/student-reports-view`,
                params: {
                    ...(studentCode && { studentCode }),
                    ...(courseId && { courseId }),
                    ...(studentId && { studentId }),
                },
            }),
            providesTags: ["Report"],
        }),
    }),
})

export const {
    useGetReportQuery,
    useSubmitAttendanceReportMutation,
    useRequestReopenReportMutation,
    useGetAttendanceReportsByDateQuery,
    useMarkReportAsReadMutation,
    useReopenReportMutation,
    useSubmitNightShiftReportMutation,
    useGetReportsByStudentQuery,
} = reportApi

export default reportApi

// StudentCourseInfo.tsx
import React, { useEffect, useState } from "react"
import { useGetStudentGroupsQuery } from "../../../apis/studentGroupApi"
import {
    useUpdateStudentApplicationDetailMutation,
    useUpdateStudentApplicationMutation,
} from "../../../apis/studentApplicationApi"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { format } from "date-fns"
import { z } from "zod"
import { Form, Button, Row, Col } from "react-bootstrap"
import { SD_CourseStatus, SD_ProcessStatus, SD_ProcessStatus_Name, SD_Role_Name } from "../../../utility/SD"
import { useSelector } from "react-redux"
import { RootState } from "../../../store/store"
import { toastNotify } from "../../../helper"
import { useGetCourseByIdQuery } from "../../../apis/courseApi"

// Form schema with studentGroupId and status as strings
const formSchema = z.object({
    studentGroupId: z.string().trim().min(1, "Nhóm là bắt buộc"),
    status: z.string().trim().min(1, "Trạng thái là bắt buộc"),
    note: z.string().trim().optional(),
})

type FormData = z.infer<typeof formSchema>

function StudentCourseInfo({ studentApplication, courseId, refetchStudent }: any) {
    const { data: groupData } = useGetStudentGroupsQuery(courseId, { skip: !courseId })
    const [updateStudentApplication, { isLoading }] = useUpdateStudentApplicationDetailMutation()
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const [isEditing, setIsEditing] = useState(false)
    const { data: courseData, isLoading: courseLoading } = useGetCourseByIdQuery(studentApplication.course.id, {
        skip: !studentApplication,
    })
    const isPast = courseData?.result?.status == SD_CourseStatus.closed

    const {
        register,
        handleSubmit,
        formState: { errors },
        reset,
    } = useForm<FormData>({
        resolver: zodResolver(formSchema),
    })

    useEffect(() => {
        // Initialize form data
        if (studentApplication) {
            const initialData = {
                studentGroupId: studentApplication.student?.studentGroups?.[0]?.id
                    ? studentApplication.student.studentGroups[0].id.toString()
                    : "",
                status: studentApplication.status?.toString() ?? SD_ProcessStatus.Approved.toString(),
                note: studentApplication.note || "",
            }
            reset(initialData)
        }
    }, [studentApplication, reset])

    const onSubmit = async (data: FormData) => {
        try {
            await updateStudentApplication({
                id: studentApplication.id,
                courseId: studentApplication.courseId,
                status: parseInt(data.status),
                note: data.note,
                studentGroupId: parseInt(data.studentGroupId),
            }).unwrap()
            toastNotify("Cập nhật thông tin khóa học thành công", "success")
            setIsEditing(false)
            refetchStudent()
        } catch (error: any) {
            console.error("Error updating student application:", error)
            toastNotify("Cập nhật thông tin khóa học thất bại", "error")
        }
    }

    const getStudentStatusOptions = () => {
        const allowedStatuses = [
            SD_ProcessStatus.Approved,
            SD_ProcessStatus.Enrolled,
            SD_ProcessStatus.Graduated,
            SD_ProcessStatus.DropOut,
        ]

        return allowedStatuses.map((status) => ({
            value: status.toString(),
            label: getStudentStatusName(status),
        }))
    }

    const getStudentStatusName = (status: SD_ProcessStatus) => {
        return status === SD_ProcessStatus.Approved
            ? SD_ProcessStatus_Name.WaitingForEnroll
            : status === SD_ProcessStatus.Enrolled
            ? SD_ProcessStatus_Name.Enrolled
            : status === SD_ProcessStatus.Graduated
            ? SD_ProcessStatus_Name.Graduated
            : SD_ProcessStatus_Name.DropOut
    }

    // Filter student groups based on gender
    const filteredGroups = groupData?.result?.filter(
        (group: any) => group.gender === studentApplication.student.gender || group.gender === null,
    )

    // Ensure that groupOptions have value as strings
    const groupOptions = filteredGroups?.map((group: any) => ({
        value: group.id.toString(),
        label: group.groupName,
    }))
    return (
        <Form onSubmit={handleSubmit(onSubmit)} className="mt-3">
            <Row className="mb-3">
                {/* Tên khóa */}
                <Col md={3}>
                    <Form.Group controlId="courseName">
                        <Form.Label>Tên khóa</Form.Label>
                        <Form.Control type="text" value={studentApplication.course?.courseName || ""} disabled />
                    </Form.Group>
                </Col>

                {/* Mã khóa sinh */}
                <Col md={3}>
                    <Form.Group controlId="studentCode">
                        <Form.Label>Mã khóa sinh</Form.Label>
                        <Form.Control type="text" value={studentApplication.studentCode || ""} disabled />
                    </Form.Group>
                </Col>

                {/* Nhóm */}
                <Col md={3}>
                    <Form.Group controlId="studentGroupId">
                        <Form.Label>Chánh</Form.Label>
                        {isEditing ? (
                            <>
                                <Form.Select {...register("studentGroupId")} isInvalid={!!errors.studentGroupId}>
                                    {groupOptions?.map((group: any) => (
                                        <option key={group.value} value={group.value}>
                                            {group.label}
                                        </option>
                                    ))}
                                </Form.Select>
                                <Form.Control.Feedback type="invalid">
                                    {errors.studentGroupId?.message}
                                </Form.Control.Feedback>
                            </>
                        ) : (
                            <Form.Control
                                type="text"
                                value={studentApplication.student?.studentGroups?.[0]?.groupName || "Chưa phân"}
                                disabled
                            />
                        )}
                    </Form.Group>
                </Col>

                {/* Trạng thái */}
                <Col md={3}>
                    <Form.Group controlId="status">
                        <Form.Label>Trạng thái</Form.Label>
                        {isEditing ? (
                            <>
                                <Form.Select {...register("status")} isInvalid={!!errors.status}>
                                    {getStudentStatusOptions().map((option) => (
                                        <option key={option.value} value={option.value}>
                                            {option.label}
                                        </option>
                                    ))}
                                </Form.Select>
                                <Form.Control.Feedback type="invalid">{errors.status?.message}</Form.Control.Feedback>
                            </>
                        ) : (
                            <Form.Control
                                type="text"
                                value={getStudentStatusName(studentApplication.status)}
                                disabled
                            />
                        )}
                    </Form.Group>
                </Col>

                {/* Ghi chú */}
                <Col md={12}>
                    <Form.Group controlId="note">
                        <Form.Label>Ghi chú</Form.Label>
                        {isEditing ? (
                            <>
                                <Form.Control as="textarea" rows={3} {...register("note")} isInvalid={!!errors.note} />
                                <Form.Control.Feedback type="invalid">{errors.note?.message}</Form.Control.Feedback>
                            </>
                        ) : (
                            <Form.Control as="textarea" rows={3} value={studentApplication.note || ""} disabled />
                        )}
                    </Form.Group>
                </Col>
                <Col md={3}>
                    <Form.Group controlId="courseName">
                        <Form.Label>Người duyệt</Form.Label>
                        <Form.Control type="text" value={studentApplication.reviewer?.userName || ""} disabled />
                    </Form.Group>
                </Col>
                <Col md={3}>
                    <Form.Group controlId="courseName">
                        <Form.Label>Ngày đăng ký</Form.Label>
                        <Form.Control
                            type="text"
                            value={format(new Date(studentApplication.applicationDate), "dd-MM-yyyy") || ""}
                            disabled
                        />
                    </Form.Group>
                </Col>
                <Col md={3}>
                    <Form.Group controlId="courseName">
                        <Form.Label>Ngày Duyệt</Form.Label>
                        <Form.Control
                            type="text"
                            value={format(new Date(studentApplication.reviewDate), "dd-MM-yyyy") || ""}
                            disabled
                        />
                    </Form.Group>
                </Col>
            </Row>
            {(currentUserRole === SD_Role_Name.SECRETARY || currentUserRole === SD_Role_Name.MANAGER) && (
                <div className="text-end">
                    {isEditing ? (
                        <>
                            <Button variant="success" type="submit" disabled={isLoading}>
                                Lưu
                            </Button>
                            <button
                                type="button"
                                className="ms-2 btn btn-secondary"
                                onClick={() => {
                                    setIsEditing(false)
                                    reset()
                                }}
                            >
                                Hủy
                            </button>
                        </>
                    ) : (
                        <button
                            type="button"
                            disabled={isPast}
                            className="btn btn-primary"
                            onClick={() => setIsEditing(true)}
                        >
                            Sửa
                        </button>
                    )}
                </div>
            )}
        </Form>
    )
}

export default StudentCourseInfo

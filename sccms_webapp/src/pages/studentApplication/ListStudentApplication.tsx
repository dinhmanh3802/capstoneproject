import React, { useEffect, useState } from "react"
import { Link, useNavigate, useLocation } from "react-router-dom"
import { MainLoader, SendEmailPopup, StudentApplicationList, StudentApplicationSearch } from "../../components/Page"
import {
    useAutoAssignApplicationsMutation,
    useGetStudentApplicationsQuery,
    useSendStudentApplicationResultMutation,
} from "../../apis/studentApplicationApi"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { useGetUsersQuery } from "../../apis/userApi"
import { SD_CourseStatus, SD_Role, SD_Role_Name } from "../../utility/SD"
import { toastNotify } from "../../helper"
import ConfirmationPopup from "../../components/commonCp/ConfirmationPopup"
import { useAutoAssignStudentsToGroupMutation, useGetStudentGroupsQuery } from "../../apis/studentGroupApi"
import { apiResponse } from "../../interfaces"
import { useGetAllEmailTemplateQuery } from "../../apis/emailTemplateApi"
import emailTemplateModel from "../../interfaces/emailTemplateModel"
import teamApi from "../../apis/teamApi"
import { useGetCourseByIdQuery } from "../../apis/courseApi"

function ListStudentApplication() {
    const listCourseFromStore = useSelector((state: RootState) => state.courseStore.courses ?? "")
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const currentUserId = useSelector((state: RootState) => state.auth.user?.userId)
    const [autoAssignApplications] = useAutoAssignApplicationsMutation()
    const [autoAssignStudentsToGroup] = useAutoAssignStudentsToGroupMutation()
    const [sendEmail] = useSendStudentApplicationResultMutation()
    const { data: listSecretary, isLoading: reviewerLoading } = useGetUsersQuery({ roleId: SD_Role.SECRETARY })
    const { data: listEmailTemplate, isLoading: emailTemplateLoading } = useGetAllEmailTemplateQuery({})
    const isLoading = !listCourseFromStore || !currentCourse || reviewerLoading || !currentUserRole || !currentUserId

    const navigate = useNavigate()
    const location = useLocation()

    const [isPopupOpenAssign, setIsPopupOpenAssign] = useState(false)
    const [isPopupOpenGroup, setIsPopupOpenGroup] = useState(false)
    const [isModalOpenSendResult, setIsModalOpenSendResult] = useState(false)
    const [clearRowsFlag, setClearRowsFlag] = useState(false) // Cờ để xóa các hàng đã chọn
    const [selectedRows, setSelectedRows] = useState<number[]>([])

    const listTemplate: emailTemplateModel[] = listEmailTemplate?.result || []

    const searchParamsFromUrl = new URLSearchParams(location.search)
    const initialSearchParams = {
        courseId: searchParamsFromUrl.get("courseId") || currentCourse?.id,
        name: searchParamsFromUrl.get("name") || "",
        phoneNumber: searchParamsFromUrl.get("phoneNumber") || "",
        status: searchParamsFromUrl.get("status") || "",
        reviewerId: searchParamsFromUrl.get("reviewerId") || "",
        gender: searchParamsFromUrl.get("gender") || "",
        parentName: searchParamsFromUrl.get("parentName") || "",
        birthDateFrom: searchParamsFromUrl.get("birthDateFrom") || "",
        birthDateTo: searchParamsFromUrl.get("birthDateTo") || "",
        teamId: searchParamsFromUrl.get("teamId") || "",
        nationalId: searchParamsFromUrl.get("nationalId") || "",
    }

    const [searchParams, setSearchParams] = useState(initialSearchParams)
    useEffect(() => {
        setSearchParams((prevParams) => ({
            ...prevParams,
            courseId: currentCourse?.id ?? 0,
        }))
    }, [currentCourse, listSecretary, currentUserRole])
    useEffect(() => {
        const newSearchParams = {
            courseId: searchParamsFromUrl.get("courseId") || currentCourse?.id,
            name: searchParamsFromUrl.get("name") || "",
            phoneNumber: searchParamsFromUrl.get("phoneNumber") || "",
            status: searchParamsFromUrl.get("status") || "",
            reviewerId: searchParamsFromUrl.get("reviewerId") || "",
            gender: searchParamsFromUrl.get("gender") || "",
            parentName: searchParamsFromUrl.get("parentName") || "",
            birthDateFrom: searchParamsFromUrl.get("birthDateFrom") || "",
            birthDateTo: searchParamsFromUrl.get("birthDateTo") || "",
            teamId: searchParamsFromUrl.get("teamId") || "",
            nationalId: searchParamsFromUrl.get("nationalId") || "",
        }

        setSearchParams(newSearchParams)
    }, [location.search, currentCourse?.id])

    // Get student applications by search params
    const {
        data: applicationData,
        isLoading: applicationLoading,
        refetch,
    } = useGetStudentApplicationsQuery(searchParams, {
        refetchOnMountOrArgChange: true,
    })
    // Gọi API để lấy thông tin khóa tu theo selectedCourseId
    const { data: courseData, isLoading: courseLoading } = useGetCourseByIdQuery(searchParams.courseId || 0, {
        skip: !searchParams,
    })
    const isCourseClosed = courseData?.result.status === SD_CourseStatus.closed
    const isCourseAvaiable =
        courseData?.result.status === SD_CourseStatus.notStarted ||
        courseData?.result.status === SD_CourseStatus.recruiting

    const handleSearch = (params: any) => {
        setSearchParams(params)
        const queryParams = new URLSearchParams(params).toString()
        navigate(`/student-applications?${queryParams}`)
    }

    const handleSecretary = async () => {
        try {
            const response: apiResponse = await autoAssignApplications(
                Number(searchParamsFromUrl.get("courseId")) || currentCourse?.id,
            )
            if (response.data?.isSuccess) {
                toastNotify("Phân chia đơn đăng ký thành công", "success")
            } else {
                toastNotify(
                    response?.error?.data?.errorMessages?.join(", ") || "Phân chia đơn đăng ký thất bại",
                    "error",
                )
            }
        } catch (error) {
            toastNotify("Có lỗi xảy ra khi phân chia đơn đăng ký")
        } finally {
            setIsPopupOpenAssign(false)
        }
    }

    const handleStudentGroup = async () => {
        try {
            const response: apiResponse = await autoAssignStudentsToGroup(
                Number(searchParamsFromUrl.get("courseId")) || currentCourse?.id,
            )
            if (response.data?.isSuccess) {
                toastNotify("Phân chia chánh thành công", "success")
                refetch()
            } else {
                toastNotify(response?.error?.data?.errorMessages?.join(", ") || "Phân chia chánh thất bại", "error")
            }
        } catch (error) {
            toastNotify("Có lỗi xảy ra khi phân chánh")
        } finally {
            setIsPopupOpenGroup(false)
        }
    }

    const handleSendResult = () => {
        if (selectedRows.length === 0) {
            toastNotify("Chọn khóa sinh để gửi email", "error")
            return
        }
        setIsModalOpenSendResult(true)
    }

    const handleSendResultConfirm = async (title: string, content: string) => {
        try {
            const response: apiResponse = await sendEmail({
                ListStudentApplicationId: selectedRows,
                CourseId: Number(searchParamsFromUrl.get("courseId")) || currentCourse?.id,
                Subject: title,
                Message: content,
            })

            if (response.data?.isSuccess) {
                toastNotify("Email đã được gửi thành công!", "success")
            } else {
                toastNotify(response?.error?.data?.errorMessages?.join(", ") || "Gửi email thất bại", "error")
            }
        } catch (error) {
            toastNotify("Đã xảy ra lỗi khi gửi email", "error")
        }
        clearSelectedRows()
        setIsModalOpenSendResult(false)
    }

    const handleSelectRows = (selected: any) => {
        const selectedIds = selected.selectedRows?.map((row: any) => row.id)
        setSelectedRows(selectedIds)
    }

    const clearSelectedRows = () => {
        setClearRowsFlag((prev) => !prev)
    }

    const { data: studentGroup, isLoading: studentGroupLoading } = useGetStudentGroupsQuery(
        searchParamsFromUrl.get("courseId") || currentCourse?.id,
        { skip: !searchParamsFromUrl.get("courseId") && !currentCourse?.id },
    )

    if (isLoading || applicationLoading || emailTemplateLoading || studentGroupLoading || courseLoading) {
        return <MainLoader />
    }
    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Danh sách đơn đăng ký của khóa sinh</h3>
            </div>
            <StudentApplicationSearch
                onSearch={handleSearch}
                courseList={listCourseFromStore}
                currentCourse={currentCourse}
                secretaryList={listSecretary?.result}
                currentUserRole={currentUserRole}
                currentUserId={currentUserId}
            />
            <div className="container text-end mt-4">
                {currentUserRole !== SD_Role_Name.SECRETARY && (
                    <button
                        className="btn btn-outline-secondary btn-sm"
                        onClick={() => setIsPopupOpenAssign(true)}
                        disabled={isCourseClosed}
                    >
                        Chia thư ký
                    </button>
                )}
                <button
                    className="btn btn-outline-secondary btn-sm ms-2"
                    onClick={() => setIsPopupOpenGroup(true)}
                    disabled={!isCourseAvaiable}
                >
                    Phân chánh
                </button>
                <button
                    className="btn btn-outline-secondary btn-sm ms-2 me-2"
                    onClick={handleSendResult}
                    disabled={isCourseClosed}
                >
                    Gửi kết quả
                </button>
            </div>

            <div className="mt-2">
                <StudentApplicationList
                    applications={applicationData?.result}
                    secretaryList={listSecretary?.result}
                    studentGroupList={studentGroup?.result}
                    onSelectUser={handleSelectRows}
                    clearSelectedRows={clearRowsFlag}
                    currentCourse={courseData?.result}
                    refetch={refetch} // Pass refetch to the child component
                />
            </div>
            <ConfirmationPopup
                isOpen={isPopupOpenAssign}
                onClose={() => setIsPopupOpenAssign(false)}
                onConfirm={handleSecretary}
                message="Bạn có chắc chắn muốn phân chia thư ký tự động cho các đơn đăng ký không?"
                title="Xác nhận phân chia thư ký"
            />
            <ConfirmationPopup
                isOpen={isPopupOpenGroup}
                onClose={() => setIsPopupOpenGroup(false)}
                onConfirm={handleStudentGroup}
                message="Bạn có chắc chắn muốn phân chánh không?"
                title="Xác nhận phân chánh"
            />
            <SendEmailPopup
                isOpen={isModalOpenSendResult}
                onClose={() => setIsModalOpenSendResult(false)}
                onConfirm={handleSendResultConfirm}
                listTemplate={listTemplate}
                select={3} // Template mặc định
                onClearSelectedRows={clearSelectedRows}
            />
        </div>
    )
}

export default ListStudentApplication

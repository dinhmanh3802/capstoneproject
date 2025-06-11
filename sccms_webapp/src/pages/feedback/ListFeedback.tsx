import { useEffect, useState } from "react"
import {
    useDeleteFeedbackMutation,
    useDeleteFeedbacksByIdsMutation,
    useGetFeedbacksQuery,
} from "../../apis/feedbackApi"
import FeedbackSearch from "../../components/Page/feedback/FeedbackSearch"
import FeedbackList from "../../components/Page/feedback/FeedbackList"
import { MainLoader } from "../../components/Page"
import { useGetCourseQuery } from "../../apis/courseApi" // Use existing query
import { useNavigate } from "react-router-dom"
import ConfirmationPopup from "../../components/commonCp/ConfirmationPopup"
import { apiResponse } from "../../interfaces"
import { toastNotify } from "../../helper"

function ListFeedback() {
    const navigate = useNavigate()
    const { data: coursesData, isLoading: courseLoading } = useGetCourseQuery({ status: 3 })
    const [isPopupOpenDelete, setIsPopupOpenDelete] = useState(false)
    const [deleteFeedback] = useDeleteFeedbackMutation()
    const [deleteFeedbackBulk] = useDeleteFeedbacksByIdsMutation()
    const [selectedFeedbackIds, setSelectedFeedbackIds] = useState<number[]>([]) // Chứa các ID cho xóa hàng loạt
    const [isPopupOpenDeleteBulk, setIsPopupOpenDeleteBulk] = useState(false)
    const searchParamsFromUrl = new URLSearchParams(location.search)
    const [selectedFeedbackId, setSelectedFeedbackId] = useState<number | null>(null)

    // Initialize searchParams only when coursesData is available
    const [searchParams, setSearchParams] = useState({
        courseId: 0, // Default to 0 until coursesData is loaded
        feedbackDateStart: searchParamsFromUrl.get("feedbackDateStart") || "",
        feedbackDateEnd: searchParamsFromUrl.get("feedbackDateEnd") || "",
    })

    // Update searchParams after coursesData is loaded
    useEffect(() => {
        if (coursesData && coursesData.result && coursesData.result.length > 0) {
            setSearchParams((prevParams) => ({
                ...prevParams,
                courseId: searchParamsFromUrl.get("courseId") || coursesData.result[0].id,
            }))
        }
    }, [coursesData])

    const handleSearch = (params: any) => {
        setSearchParams(params)
        const queryParams = new URLSearchParams(params).toString()
        navigate(`/feedback?${queryParams}`)
    }

    const handleDeleteFeedback = async () => {
        if (selectedFeedbackId === null) return
        try {
            const response: apiResponse = await deleteFeedback(selectedFeedbackId)
            if (response.data?.statusCode === 204) {
                toastNotify("Xóa phản hồi thành công", "success")
            } else {
                toastNotify("Xóa phản hồi thất bại", "error")
            }
        } catch (error) {
            toastNotify("Đã xảy ra lỗi khi xóa bài đăng", "error")
        } finally {
            setIsPopupOpenDelete(false)
        }
    }

    const handleDeleteConfirmBulk = async () => {
        if (selectedFeedbackIds.length === 0) return
        try {
            const response: apiResponse = await deleteFeedbackBulk(selectedFeedbackIds)

            if (response.data?.statusCode === 204) {
                toastNotify("Xóa phản hồi hàng loạt thành công", "success")
            } else {
                toastNotify("Xóa phản hồi hàng loạt thất bại", "error")
            }
        } catch (error) {
            toastNotify("Đã xảy ra lỗi khi xóa phản hồi hàng loạt", "error")
        } finally {
            setIsPopupOpenDeleteBulk(false)
        }
    }

    const { data: feedbackData, isLoading: paramLoading } = useGetFeedbacksQuery(searchParams)

    if (courseLoading || paramLoading) {
        return <MainLoader />
    }

    return (
        <div className="container">
            <h3 className="fw-bold primary-color">Danh sách phản hồi</h3>
            <FeedbackSearch onSearch={handleSearch} courseList={coursesData.result} />
            <FeedbackList
                feedbacks={feedbackData?.result || []}
                onDelete={(feedbackId: number) => {
                    setSelectedFeedbackId(feedbackId)
                    setIsPopupOpenDelete(true)
                }}
                onBulkDelete={(feedbackIds: number[]) => {
                    setSelectedFeedbackIds(feedbackIds)
                    setIsPopupOpenDeleteBulk(true)
                }}
            />
            <ConfirmationPopup
                isOpen={isPopupOpenDelete}
                onClose={() => setIsPopupOpenDelete(false)}
                onConfirm={handleDeleteFeedback}
                message="Bạn có chắc chắn muốn xóa phản hồi này không?"
                title="Xác nhận xóa bài đăng"
            />
            <ConfirmationPopup
                isOpen={isPopupOpenDeleteBulk}
                onClose={() => setIsPopupOpenDeleteBulk(false)}
                onConfirm={handleDeleteConfirmBulk}
                message="Bạn có chắc chắn muốn xóa các phản hồi đã chọn không?"
                title="Xác nhận xóa hàng loạt phản hồi"
            />
        </div>
    )
}

export default ListFeedback

import { useState } from "react"
import { Link, useNavigate, useLocation } from "react-router-dom"
import { MainLoader } from "../../components/Page"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { useGetPostsQuery, useDeletePostMutation } from "../../apis/postApi"
import { toastNotify } from "../../helper"
import { apiResponse } from "../../interfaces"
import PostSearch from "../../components/Page/post/PostSearch"
import ConfirmationPopup from "../../components/commonCp/ConfirmationPopup"
import PostTable from "../../components/Page/post/PostTable"

function PostList() {
    const navigate = useNavigate()
    const location = useLocation()
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)
    const [deletePost] = useDeletePostMutation()
    const isLoading = !currentUserRole
    const [isPopupOpenDelete, setIsPopupOpenDelete] = useState(false)
    const [selectedPostId, setSelectedPostId] = useState<number | null>(null)

    // Initial search params based on URL or defaults
    const searchParamsFromUrl = new URLSearchParams(location.search)
    const initialSearchParams = {
        postType: searchParamsFromUrl.get("postType") || "",
        title: searchParamsFromUrl.get("title") || "",
        createdBy: searchParamsFromUrl.get("createdBy") || "",
        status: searchParamsFromUrl.get("status") || "",
        postDateStart: searchParamsFromUrl.get("postDateStart") || "",
        postDateEnd: searchParamsFromUrl.get("postDateEnd") || "",
    }

    const [searchParams, setSearchParams] = useState(initialSearchParams)
    const { data: postsData, isLoading: postsLoading } = useGetPostsQuery(searchParams)

    const handleSearch = (params: any) => {
        setSearchParams(params)
        const queryParams = new URLSearchParams(params).toString()
        navigate(`/post?${queryParams}`)
    }

    const handleDeletePost = async () => {
        if (selectedPostId === null) return
        try {
            const response: apiResponse = await deletePost(selectedPostId)
            if (response.data?.statusCode === 204) {
                toastNotify("Xóa bài đăng thành công", "success")
            } else {
                toastNotify("Xóa bài đăng thất bại", "error")
            }
        } catch (error) {
            toastNotify("Đã xảy ra lỗi khi xóa bài đăng", "error")
        } finally {
            setIsPopupOpenDelete(false)
        }
    }

    if (isLoading || postsLoading) {
        return <MainLoader />
    }

    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Danh sách bài đăng</h3>
            </div>
            <PostSearch onSearch={handleSearch} />
            <div className="container text-end mt-4">
                <Link to="/post/create" className="btn btn-outline-primary btn-sm ms-2 me-2">
                    + Thêm mới
                </Link>
            </div>
            <div className="mt-2">
                <PostTable
                    posts={postsData?.result}
                    // onEdit={(postId) => navigate(`/posts/edit/${postId}`)}
                    onDelete={(postId) => {
                        setSelectedPostId(postId)
                        setIsPopupOpenDelete(true)
                    }}
                />
            </div>

            <ConfirmationPopup
                isOpen={isPopupOpenDelete}
                onClose={() => setIsPopupOpenDelete(false)}
                onConfirm={handleDeletePost}
                message="Bạn có chắc chắn muốn xóa bài đăng này không?"
                title="Xác nhận xóa bài đăng"
            />
        </div>
    )
}

export default PostList

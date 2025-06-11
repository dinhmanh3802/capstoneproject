// src/pages/post/PostDetail.tsx
import React from "react"
import { useParams } from "react-router-dom"
import { useGetPostByIdQuery } from "../../apis/postApi"
import { MainLoader } from "../../components/Page"
import PostInfo from "../../components/Page/post/PostInfo"
import { toastNotify } from "../../helper"

function PostDetail() {
    const { id } = useParams<{ id: string }>()
    const { data, isLoading, isError, error } = useGetPostByIdQuery(Number(id))
    if (isLoading) {
        return <MainLoader />
    }

    if (isError) {
        toastNotify("Đã xảy ra lỗi khi tải thông tin bài đăng", "error")
        return <p>Đã xảy ra lỗi khi tải thông tin bài đăng.</p>
    }

    const post = data?.result

    if (!post) {
        return <p>Không tìm thấy bài đăng.</p>
    }

    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Chi tiết Bài đăng</h3>
            </div>
            <PostInfo post={post} />
        </div>
    )
}

export default PostDetail

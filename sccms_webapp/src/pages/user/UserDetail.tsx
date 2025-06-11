// src/pages/user/UserDetail.tsx
import React from "react"
import { useParams, useNavigate } from "react-router-dom"
import { useGetUserByIdQuery } from "../../apis/userApi"
import { MainLoader } from "../../components/Page"
import UserInfo from "../../components/Page/user/UserInfo"
import { toastNotify } from "../../helper"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { SD_Role, SD_Role_Name } from "../../utility/SD"

function UserDetail() {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()
    const { data, isLoading, isError, error } = useGetUserByIdQuery(Number(id))
    const currentUserRoleName = useSelector((state: RootState) => state.auth.user?.role)

    if (isLoading) {
        return <MainLoader />
    }

    if (isError) {
        toastNotify("Đã xảy ra lỗi khi tải thông tin người dùng", "error")
        return <p>Đã xảy ra lỗi khi tải thông tin người dùng.</p>
    }

    const user = data?.result

    if (!user) {
        return <p>Không tìm thấy người dùng.</p>
    }

    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Chi tiết Người dùng</h3>
            </div>
            <UserInfo user={user} />
        </div>
    )
}

export default UserDetail

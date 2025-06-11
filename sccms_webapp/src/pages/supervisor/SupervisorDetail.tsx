import React from "react"
import { useParams, useNavigate } from "react-router-dom"
import { useGetSupervisorByIdQuery } from "../../apis/supervisorApi"
import { MainLoader, SupervisorInfo } from "../../components/Page"

import { toastNotify } from "../../helper"

function SupervisorDetail() {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()
    const { data, isLoading, isError, error } = useGetSupervisorByIdQuery(Number(id))

    if (isLoading) {
        return <MainLoader />
    }

    if (isError) {
        toastNotify("Đã xảy ra lỗi khi tải thông tin huynh trưởng", "error")
        return <p>Đã xảy ra lỗi khi tải thông tin huynh trưởng.</p>
    }

    const supervisor = data?.result

    if (!supervisor) {
        return <p>Không tìm thấy huynh trưởng.</p>
    }

    return (
        <div className="container">
            <div className="mt-0 mb-2">
                <h3 className="fw-bold primary-color">Chi tiết Huynh trưởng</h3>
            </div>
            <SupervisorInfo supervisor={supervisor} />
        </div>
    )
}

export default SupervisorDetail

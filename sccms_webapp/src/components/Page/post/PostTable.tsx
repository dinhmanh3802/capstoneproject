import React, { useState } from "react"
import DataTable, { TableColumn } from "react-data-table-component"
import { OverlayTrigger, Tooltip } from "react-bootstrap"
import { Link } from "react-router-dom"

// Định nghĩa kiểu dữ liệu cho một bài viết
interface Post {
    id: number
    postType: number
    title: string
    content: string
    status: number // Thêm trường status để hiển thị trạng thái
}

interface PostTableProps {
    posts: Post[]
    onDelete: (id: number) => void
}

// Hàm định dạng trạng thái với các badge màu sắc
const getPostStatusText = (status: number) => {
    switch (status) {
        case 0: // Bản nháp
            return <span className="badge bg-secondary text-white">Bản nháp</span>
        case 1: // Hiển thị
            return <span className="badge bg-success text-white">Hiển thị</span>
        default:
            return <span className="badge bg-light text-dark">Không xác định</span>
    }
}

const PostTable: React.FC<PostTableProps> = ({ posts, onDelete }) => {
    const [currentPage, setCurrentPage] = useState(1) // Trang hiện tại
    const [rowsPerPage, setRowsPerPage] = useState(10) // Số hàng mỗi trang

    // Tính toán dữ liệu hiển thị trên trang hiện tại
    const paginatedPosts = posts.slice((currentPage - 1) * rowsPerPage, currentPage * rowsPerPage)

    // Định nghĩa các cột cho bảng
    const columns: TableColumn<Post>[] = [
        {
            name: "Mục",
            selector: (row) => row.postType.toString(),
            sortable: true,
            minWidth: "12rem",
            cell: (row) => {
                switch (row.postType) {
                    case 0:
                        return "Giới thiệu"
                    case 1:
                        return "Hoạt động khoá tu"
                    case 2:
                        return "Hướng dẫn đăng kí"
                    default:
                        return "Không xác định"
                }
            },
        },
        {
            name: "Tiêu đề",
            selector: (row) => row.title,
            sortable: true,
            minWidth: "40rem", // Tăng kích thước để hiển thị đầy đủ Tiêu đề
            wrap: true, // Cho phép văn bản xuống dòng
        },
        // {
        //     name: "Nội dung",
        //     selector: (row) => row.content,
        //     sortable: false,
        //     minWidth: "30rem", // Tăng kích thước cột Nội dung để trở thành lớn nhất
        //     cell: (row) => (row.content.length > 100 ? `${row.content.substring(0, 100)}...` : row.content),
        // },
        {
            name: "Trạng thái",
            selector: (row) => row.status.toString(),
            sortable: true,
            minWidth: "3rem",
            cell: (row) => getPostStatusText(row.status), // Sử dụng hàm định dạng trạng thái
        },
        {
            name: "Thao tác",
            cell: (row) => (
                <div className="d-flex">
                    <OverlayTrigger placement="top" overlay={<Tooltip>Xem chi tiết</Tooltip>}>
                        <Link to={`/post/${row.id}`} className="btn btn-outline-primary btn-sm m-1">
                            <i className="bi bi-eye"></i>
                        </Link>
                    </OverlayTrigger>
                    <OverlayTrigger placement="top" overlay={<Tooltip>Chỉnh sửa</Tooltip>}>
                        <Link to={`/post/${row.id}?edit=1`} className="btn btn-outline-warning btn-sm m-1">
                            <i className="bi bi-pencil"></i>
                        </Link>
                    </OverlayTrigger>
                    <OverlayTrigger placement="top" overlay={<Tooltip>Xóa</Tooltip>}>
                        <button className="btn btn-outline-danger btn-sm m-1" onClick={() => onDelete(row.id)}>
                            <i className="bi bi-trash"></i>
                        </button>
                    </OverlayTrigger>
                </div>
            ),
            ignoreRowClick: true,
            allowOverflow: true,
            button: true,
            width: "10rem",
        },
    ]

    return (
        <div className="card">
            <div className="card-body">
                <DataTable
                    columns={columns}
                    data={paginatedPosts}
                    pagination
                    paginationServer
                    paginationTotalRows={posts.length}
                    paginationPerPage={rowsPerPage}
                    onChangePage={(page) => setCurrentPage(page)}
                    onChangeRowsPerPage={(newPerPage) => {
                        setRowsPerPage(newPerPage)
                        setCurrentPage(1) // Reset về trang 1 khi thay đổi số hàng mỗi trang
                    }}
                    customStyles={{
                        headCells: {
                            style: {
                                fontSize: "15px",
                                fontWeight: "bold",
                            },
                        },
                        rows: {
                            style: {
                                fontSize: "14px",
                                padding: "0.5rem",
                            },
                        },
                    }}
                    responsive
                    striped
                />
            </div>
        </div>
    )
}

export default PostTable

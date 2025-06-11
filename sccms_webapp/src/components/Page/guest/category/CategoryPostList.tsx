import { useParams, useNavigate } from "react-router-dom"
import { useGetPostsQuery } from "../../../../apis/postApi"
import { getPostType } from "../../../../helper/getPostType"
import LargePost from "../mainpage/LargePost"
import SmallPost from "../mainpage/SmallPost"
import MediumPost from "../mainpage/MediumPost"
import { useState, useEffect } from "react"
import { MainLoader } from "../.."

const CategoryPostList = () => {
    const { categoryID: rawCategoryID } = useParams() // Lấy raw categoryID từ URL
    const navigate = useNavigate()

    // Kiểm tra và validate categoryID, nếu ngoài vùng thì đưa về 0
    const categoryID =
        isNaN(Number(rawCategoryID)) || Number(rawCategoryID) < 0 || Number(rawCategoryID) > 2
            ? 0
            : Number(rawCategoryID)

    // Điều hướng URL về `/home/category/0` nếu categoryID ngoài vùng
    useEffect(() => {
        if (categoryID === 0 && rawCategoryID !== "0") {
            navigate("/home/category/0", { replace: true })
        }
    }, [categoryID, rawCategoryID, navigate])

    const [pageSize, setPageSize] = useState(7) // Số lượng bài hiện tại, mặc định là 7

    // Reset pageSize về 7 mỗi khi categoryID thay đổi
    useEffect(() => {
        setPageSize(7)
    }, [categoryID])

    const { data, isLoading } = useGetPostsQuery({ pageNumber: 1, pageSize, postType: categoryID, status: 1 })

    if (isLoading) return <MainLoader />

    // Hàm để tải thêm 6 bài khi nhấn nút "Xem thêm"
    const handleLoadMore = () => {
        setPageSize((prevSize) => prevSize + 6) // Tăng số lượng bài cần tải lên 6
    }

    const noMorePosts = data?.result?.length < pageSize // Kiểm tra nếu không còn bài nào để tải thêm

    return (
        <div className="row" style={{ marginTop: "20px" }}>
            <div className="col-12">
                <div className="section-title">
                    <h4 className="m-0 text-uppercase font-weight-bold mt-2">{getPostType(categoryID)}</h4>
                </div>
            </div>

            {data?.result?.slice(0, 1)?.map((post: any, index: any) => (
                <div className="col-lg-12" key={index}>
                    <LargePost post={post} />
                </div>
            ))}

            {data?.result?.slice(1, 3)?.map((post: any, index: any) => (
                <div className="col-lg-6" key={index}>
                    <MediumPost post={post} />
                </div>
            ))}

            {data?.result?.slice(3)?.map((post: any, index: any) => (
                <div className="col-lg-6" key={index}>
                    <SmallPost post={post} />
                </div>
            ))}

            {/* Nút xem thêm */}
            <div className="col-12 d-flex justify-content-center mt-3">
                <button
                    className="btn btn-primary border rounded px-4"
                    onClick={handleLoadMore}
                    disabled={noMorePosts} // Vô hiệu hóa khi không còn bài đăng nào nữa
                >
                    Xem thêm
                </button>
            </div>
        </div>
    )
}

export default CategoryPostList

import React from "react"
import { useGetPostsQuery } from "../../../../apis/postApi"
import SmallPost from "./SmallPost"
import { getPostType } from "../../../../helper/getPostType"
import LargePost from "./LargePost"
import { Link } from "react-router-dom"
import { MainLoader } from "../.."

const MainAnnouncementList: React.FC = () => {
    const { data, isLoading } = useGetPostsQuery({ pageNumber: 1, pageSize: 5, postType: 2, status: 1 })

    if (isLoading) return <MainLoader />

    return (
        <div className="row mt-3">
            <div className="col-12">
                <div className="section-title">
                    <h4 className="m-0 text-uppercase font-weight-bold mt-2">
                        {getPostType(data?.result[0]?.postType)}
                    </h4>
                    <Link
                        to={`/home/category/${data?.result[0]?.postType}`}
                        className="text-secondary font-weight-medium text-decoration-none"
                        onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
                    >
                        Xem thêm
                    </Link>
                </div>
            </div>

            {data?.result?.slice(0, 1)?.map((post: any, index: any) => (
                <div className="col-lg-12" key={index}>
                    <LargePost post={post} />
                </div>
            ))}

            {data?.result?.slice(1)?.map((post: any, index: any) => (
                <div className="col-lg-6" key={index}>
                    <SmallPost post={post} />
                </div>
            ))}
        </div>
    )
}

export default MainAnnouncementList

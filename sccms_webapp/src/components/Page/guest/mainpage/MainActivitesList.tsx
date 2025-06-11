import React from "react"
import { useGetPostsQuery } from "../../../../apis/postApi"
import MediumPost from "./MediumPost"
import SmallPost from "./SmallPost"
import { getPostType } from "../../../../helper/getPostType"

import { Link } from "react-router-dom"
import { MainLoader } from "../.."

const MainActivitiesList: React.FC = () => {
    const { data, isLoading } = useGetPostsQuery({ pageNumber: 1, pageSize: 6, postType: 1, status: 1 })

    if (isLoading) return <MainLoader />

    return (
        <div className="row">
            <div className="col-12">
                <div className="section-title">
                    <h4 className="m-0 text-uppercase font-weight-bold">{getPostType(data?.result[0]?.postType)}</h4>
                    <Link
                        to={`/home/category/${data?.result[0]?.postType}`}
                        className="text-secondary font-weight-medium text-decoration-none"
                        onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
                    >
                        Xem thêm
                    </Link>
                </div>
            </div>

            {data?.result?.slice(0, 2)?.map((post: any, index: any) => (
                <div className="col-lg-6" key={index}>
                    <MediumPost post={post} />
                </div>
            ))}

            {data?.result?.slice(2)?.map((post: any, index: any) => (
                <div className="col-lg-6" key={index}>
                    <SmallPost post={post} />
                </div>
            ))}
        </div>
    )
}

export default MainActivitiesList

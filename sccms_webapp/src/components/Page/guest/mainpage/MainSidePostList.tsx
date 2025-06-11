import React from "react"
import { useGetPostsQuery } from "../../../../apis/postApi"
import SidePost from "./SidePost"
import { MainLoader } from "../.."

const MainSidePostList: React.FC = () => {
    const { data, isLoading } = useGetPostsQuery({ pageNumber: 1, pageSize: 5, status: 1 })

    if (isLoading) return <MainLoader />

    return (
        <div className="mb-3">
            <div className="section-title mb-0">
                <h4 className="m-0 text-uppercase font-weight-bold">Bài đăng gần đây</h4>
            </div>
            <div className="bg-white border border-top-0 p-3">
                {data?.result?.map((post: any, index: any) => (
                    <SidePost key={index} post={post} />
                ))}
            </div>
        </div>
    )
}

export default MainSidePostList

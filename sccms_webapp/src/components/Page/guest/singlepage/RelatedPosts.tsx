import React from "react"
import { Link } from "react-router-dom"
import { useGetPostsQuery } from "../../../../apis/postApi"
import RelatedPostItem from "./RelatedPostItem"
import { getPostType } from "../../../../helper/getPostType"
import { MainLoader } from "../.."

interface RelatedPostsProps {
    categoryID: number
    currentPostID: number
}

const RelatedPosts: React.FC<RelatedPostsProps> = ({ categoryID, currentPostID }) => {
    const { data, isLoading } = useGetPostsQuery({ pageNumber: 1, pageSize: 6, postType: categoryID, status: 1 })

    if (isLoading) return <MainLoader />

    const relatedPosts = data?.result?.filter((post: any) => post.id !== currentPostID)

    return (
        <div className="mt-5">
            <div className="row m-0">
                <div className="section-title">
                    <h4 className="text-uppercase font-weight-bold mt-2">Tin liên quan</h4>
                </div>
            </div>
            <div className="row">
                {relatedPosts?.map((post: any) => (
                    <div className="col-12 col-md-12 mb-2" key={post.id}>
                        <RelatedPostItem post={post} />
                    </div>
                ))}
            </div>
        </div>
    )
}

export default RelatedPosts

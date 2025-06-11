import React from "react"
import { getPostType } from "../../../../helper/getPostType"
import { Link } from "react-router-dom"

interface PostProps {
    post: {
        id: number
        image: string
        postType: number
        dateCreated: string
        title: string
        content: string // Thêm trường content
    }
}

const RelatedPostItem: React.FC<PostProps> = ({ post }) => {
    const stripHtmlTags = (html) => {
        const doc = new DOMParser().parseFromString(html, "text/html")
        return doc.body.textContent || ""
    }
    return (
        <div className="d-flex align-items-center bg-white mb-1" style={{ height: 90 }}>
            <img
                className="img-fluid h-100"
                src={post.image}
                alt="related post"
                style={{ objectFit: "cover", width: "90px" }}
            />
            <div className="w-100 h-100 px-3 d-flex flex-column justify-content-center border border-left-0">
                <div className="mb-2">
                    {/* Link đến trang category */}
                    <Link
                        to={`/home/category/${post.postType}`}
                        className="badge badge-primary text-uppercase font-weight-semi-bold p-1 mr-2"
                        onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
                    >
                        {getPostType(post.postType)}
                    </Link>
                    {/* Hiển thị ngày */}
                    <span className="text-body">
                        <small>{new Date(post.dateCreated).toLocaleDateString()}</small>
                    </span>
                </div>
                {/* Link đến trang post sử dụng id */}
                <Link
                    to={`/home/post/${post.id}`}
                    className="text-secondary text-uppercase font-weight-bold"
                    onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
                >
                    <p
                        className="m-0"
                        style={{
                            display: "-webkit-box",
                            WebkitLineClamp: 1,
                            WebkitBoxOrient: "vertical",
                            overflow: "hidden",
                            textOverflow: "ellipsis",
                            fontSize: "1rem",
                            textTransform: "capitalize",
                        }}
                    >
                        {post.title}
                    </p>
                </Link>
                {/* Nội dung giới hạn 2 dòng */}
                <div
                    className="m-0 post-content"
                    style={{
                        display: "-webkit-box",
                        WebkitLineClamp: 1,
                        WebkitBoxOrient: "vertical",
                        overflow: "hidden",
                        textOverflow: "ellipsis",
                        fontFamily: "Montserrat, sans-serif",
                        fontSize: "14px",
                    }}
                >
                    {stripHtmlTags(post.content)}
                </div>
            </div>
        </div>
    )
}

export default RelatedPostItem

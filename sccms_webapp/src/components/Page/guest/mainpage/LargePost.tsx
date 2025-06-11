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
        content: string
    }
}

const LargePost: React.FC<PostProps> = ({ post }) => {
    const stripHtmlTags = (html) => {
        const doc = new DOMParser().parseFromString(html, "text/html")
        return doc.body.textContent || ""
    }

    return (
        <div className="row news-lg mx-0 mb-3">
            <div className="col-md-6 h-100 px-0">
                <img
                    style={{
                        width: "100%", // Đảm bảo hình ảnh luôn phủ đầy bề ngang
                        aspectRatio: "1", // Giữ tỷ lệ 1:1 giữa width và height
                        objectFit: "cover", // Căn giữa và cắt ảnh để tránh méo ảnh
                    }}
                    className="img-fluid h-100"
                    src={post.image}
                    alt="large post"
                />
            </div>
            <div className="col-md-6 d-flex flex-column justify-content-center border bg-white h-100 px-0">
                <div className="p-4">
                    <div className="mb-2">
                        {/* Link đến trang category */}
                        <Link
                            to={`/home/category/${post.postType}`}
                            className="badge badge-primary text-uppercase font-weight-semi-bold p-2 mr-2"
                            onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
                        >
                            {getPostType(post.postType)}
                        </Link>
                        <span className="text-body">
                            <small>{new Date(post.dateCreated).toLocaleDateString()}</small>
                        </span>
                    </div>
                    {/* Link đến trang post */}
                    <Link
                        to={`/home/post/${post.id}`}
                        className="d-block mb-3 text-secondary text-uppercase font-weight-bold"
                        onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
                        style={{
                            display: "-webkit-box",
                            WebkitLineClamp: 4,
                            WebkitBoxOrient: "vertical",
                            overflow: "hidden",
                            textOverflow: "ellipsis",
                        }}
                    >
                        <p
                            className="m-0 p-0"
                            style={{
                                display: "-webkit-box",
                                WebkitLineClamp: 2,
                                WebkitBoxOrient: "vertical",
                                overflow: "hidden",
                                textOverflow: "ellipsis",
                            }}
                        >
                            {post.title}
                        </p>
                    </Link>
                    <div
                        className="m-0 post-content"
                        style={{
                            display: "-webkit-box",
                            WebkitLineClamp: 4,
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
        </div>
    )
}

export default LargePost

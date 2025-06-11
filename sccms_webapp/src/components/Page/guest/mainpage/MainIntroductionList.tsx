import React from "react"
import { useGetPostsQuery } from "../../../../apis/postApi"
import { getPostType } from "../../../../helper/getPostType"
import { MainLoader } from "../.."
import { Link } from "react-router-dom"

const MainIntroductionList: React.FC = () => {
    const { data, isLoading } = useGetPostsQuery({ pageNumber: 1, pageSize: 5, postType: 0, status: 1 })
    if (isLoading) return <MainLoader />

    return (
        <div>
            <div className="container-fluid">
                <div className="row">
                    <div className="col-lg-7 px-0">
                        <div className="position-relative overflow-hidden" style={{ height: 500 }}>
                            <img
                                className="img-fluid w-100 h-100"
                                src={data?.result[0]?.image}
                                style={{ objectFit: "cover" }}
                                alt="Main post"
                            />
                            <div className="overlay">
                                <div className="mb-2">
                                    {/* Link đến trang category */}
                                    <Link
                                        to={`/home/category/${data?.result[0]?.postType}`}
                                        className="badge badge-primary text-uppercase font-weight-semi-bold p-2 mr-2"
                                    >
                                        {getPostType(data?.result[0]?.postType)}
                                    </Link>
                                    <span className="text-white">
                                        <small>{new Date(data?.result[0]?.dateCreated).toLocaleDateString()}</small>
                                    </span>
                                </div>
                                {/* Link đến trang post */}
                                <Link
                                    to={`/home/post/${data?.result[0]?.id}`}
                                    className="h6 m-0 text-white font-weight-semi-bold"
                                    style={{ textTransform: "capitalize" }}
                                >
                                    {data?.result[0]?.title}
                                </Link>
                            </div>
                        </div>
                    </div>

                    {/* Các bài viết còn lại */}
                    <div className="col-lg-5 px-0">
                        <div className="row mx-0">
                            {data?.result.slice(1)?.map((post: any, index: any) => (
                                <div className="col-md-6 px-0" key={index}>
                                    <div className="position-relative overflow-hidden" style={{ height: 250 }}>
                                        <img
                                            className="img-fluid w-100 h-100"
                                            src={post.image}
                                            style={{ objectFit: "cover" }}
                                            alt={`Post ${index + 1}`}
                                        />
                                        <div className="overlay">
                                            <div className="mb-2">
                                                {/* Link đến trang category */}
                                                <Link
                                                    to={`/home/category/${post.postType}`}
                                                    className="badge badge-primary text-uppercase font-weight-semi-bold p-2 mr-2"
                                                >
                                                    {getPostType(post.postType)}
                                                </Link>
                                                <span className="text-white">
                                                    <small>{new Date(post.dateCreated).toLocaleDateString()}</small>
                                                </span>
                                            </div>
                                            {/* Link đến trang post */}
                                            <Link
                                                to={`/home/post/${post.id}`}
                                                className="h6 m-0 text-white font-weight-semi-bold"
                                                style={{ textTransform: "capitalize" }}
                                            >
                                                {post.title}
                                            </Link>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
                {/* Nút Xem thêm */}
                <div className="d-flex justify-content-end mt-3">
                    <Link
                        to={`/home/category/${data?.result[0]?.postType}`}
                        className="font-weight-semi-bold"
                        style={{
                            marginRight: "25px",
                            fontStyle: "italic",
                            color: "black",
                        }}
                        onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
                    >
                        Xem thêm...
                    </Link>
                </div>
            </div>
        </div>
    )
}

export default MainIntroductionList

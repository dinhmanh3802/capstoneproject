import { Link } from "react-router-dom"
import { useGetPostsQuery } from "../../../apis/postApi"
import { getPostType } from "../../../helper/getPostType"
import { MainLoader } from ".."

const Footer = () => {
    const { data, isLoading } = useGetPostsQuery({ pageNumber: 1, pageSize: 3, status: 1 })

    if (isLoading) return <MainLoader />

    return (
        <div>
            <div className="container-fluid bg-dark pt-5 px-sm-3 px-md-5 mt-3">
                <div className="row">
                    <div className="col-lg-3 col-md-6 mb-5">
                        <h5 className="mb-4 text-white text-uppercase font-weight-bold">Liên hệ</h5>
                        <p className="font-weight-medium">
                            <i className="fa fa-map-marker-alt mr-2" />
                            Cổ Loan, Cổ Loa, Ninh Bình
                        </p>
                        <p className="font-weight-medium">
                            <i className="fa fa-phone-alt mr-2" />
                            +84969583865
                        </p>
                        <p className="font-weight-medium">
                            <i className="fa fa-envelope mr-2" />
                            chuacoloan@gmail.com
                        </p>
                    </div>
                    <div className="col-lg-9 col-md-9">
                        <h5 className="mb-4 text-white text-uppercase font-weight-bold">Tin nổi bật</h5>
                        <div className="row">
                            {data?.result?.map((post: any, index: any) => (
                                <div className="col-4 mb-3" key={index}>
                                    <div className="mb-2">
                                        <Link
                                            to={`/home/category/${post.postType}`}
                                            className="badge badge-primary text-uppercase font-weight-semi-bold p-1 mr-2"
                                            onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
                                        >
                                            {getPostType(post.postType)}
                                        </Link>
                                        <span className="text-body">
                                            <small>{new Date(post.dateCreated).toLocaleDateString()}</small>
                                        </span>
                                    </div>
                                    <Link
                                        to={`/home/post/${post.id}`}
                                        className="small text-body text-uppercase font-weight-medium"
                                        onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
                                    >
                                        {post.title}
                                    </Link>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            </div>
            <div className="container-fluid py-4 px-sm-3 px-md-5" style={{ background: "#111111" }}>
                <p className="m-0 text-center">
                    © <a href="#">CoLoanPagoda</a>. All Rights Reserved. Design by{" "}
                    <a href="https://htmlcodex.com">G51 Team</a>
                </p>
            </div>
        </div>
    )
}

export default Footer

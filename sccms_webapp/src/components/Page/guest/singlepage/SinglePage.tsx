import React, { useState, useEffect, useRef } from "react"
import { useParams, Link } from "react-router-dom"
import { useGetPostByIdQuery } from "../../../../apis/postApi"
import MainSidePostList from "../mainpage/MainSidePostList"
import { getPostType } from "../../../../helper/getPostType"
import RelatedPosts from "./RelatedPosts"
import ImageSlider from "./ImageSlider" // Import ImageSlider component
import { MainLoader } from "../.."

interface PostData {
    id: number
    title: string
    image: string
    content: string
    postType: string
    dateCreated: string
}

interface ImageData {
    src: string
    caption: string
}

interface SinglePageParams {
    postId: string
}

const SinglePage: React.FC = () => {
    const { postId } = useParams()
    const { data, isLoading, error } = useGetPostByIdQuery(Number(postId) || 0)
    const contentContainerRef = useRef<HTMLDivElement | null>(null)

    const [isSliderOpen, setSliderOpen] = useState(false)
    const [currentImageIndex, setCurrentImageIndex] = useState(0)
    const [images, setImages] = useState<ImageData[]>([])

    // Set main image once data is available
    useEffect(() => {
        if (data) {
            const mainImage = { src: data.result.image, caption: "" }
            setImages([mainImage]) // Add main image to images array
        }
    }, [data])

    // Extract images and captions from HTML content and attach click events
    useEffect(() => {
        if (contentContainerRef.current && data) {
            const first = Array.from(contentContainerRef.current.querySelectorAll("figure.image"))
            const another = Array.from(contentContainerRef.current.querySelectorAll("img"))
            const contentFigures = first.concat(another)
            const newImages = contentFigures?.map((figure) => {
                let img
                if (figure.querySelector("img")) {
                    img = figure.querySelector("img")
                } else {
                    img = figure
                }
                const caption = figure.querySelector("figcaption")?.innerHTML || "" // Get caption or empty string
                return { src: img?.src || "", caption } // Store both image src and caption
            })

            // Only update images if new images are found
            if (newImages.length > 0) {
                setImages((prevImages) => {
                    return [...prevImages, ...newImages]
                })
            }

            // Add click event for each image in the content
            contentContainerRef.current.querySelectorAll("img").forEach((img, index) => {
                img.addEventListener("click", () => {
                    setCurrentImageIndex(index + 1)
                    setSliderOpen(true)
                })
            })
        }

        // Cleanup function to remove event listeners when component unmounts
        return () => {
            contentContainerRef.current?.querySelectorAll("img").forEach((img) => {
                img.removeEventListener("click", () => {})
            })
        }
    }, [data])

    if (isLoading) return <MainLoader />
    if (error || !data) return <div>Error</div>

    const { id, title, image, content, postType, dateCreated } = data.result as PostData

    const openSlider = (index: number) => {
        setCurrentImageIndex(index)
        setSliderOpen(true)
    }

    const closeSlider = () => {
        setSliderOpen(false)
    }

    return (
        <div>
            <div className="container-fluid">
                <div className="container">
                    <div className="row">
                        <div className="col-lg-8">
                            <div className="mb-3 mt-4">
                                <Link
                                    to="/home"
                                    className="text-secondary font-weight-semi-bold"
                                    onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
                                >
                                    Trang chủ
                                </Link>
                                <span className="mx-2"> &gt;</span>
                                <Link
                                    to={`/home/category/${postType}`}
                                    className="text-secondary font-weight-semi-bold"
                                    onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}
                                >
                                    {/* @ts-ignore */}
                                    {getPostType(postType)}
                                </Link>
                            </div>
                            <div className="position-relative mb-3">
                                <img
                                    className="img-fluid w-100"
                                    src={image}
                                    alt="Post image"
                                    style={{ objectFit: "cover" }}
                                    onClick={() => openSlider(0)}
                                />
                                <div className="bg-white border border-top-0 p-4">
                                    <div className="mb-3">
                                        <p className="text-body">{new Date(dateCreated).toLocaleDateString()}</p>
                                    </div>
                                    <h1 className="mb-3 text-secondary text-uppercase font-weight-bold">{title}</h1>
                                    <div
                                        dangerouslySetInnerHTML={{ __html: content }}
                                        className="content-container"
                                        ref={contentContainerRef}
                                    />
                                </div>
                            </div>
                            {/* @ts-ignore */}
                            <RelatedPosts categoryID={postType} currentPostID={id} />
                        </div>
                        <div className="col-lg-4">
                            <MainSidePostList />
                        </div>
                    </div>
                </div>
            </div>
            {isSliderOpen && (
                <ImageSlider images={images} currentImageIndex={currentImageIndex} onClose={closeSlider} />
            )}
        </div>
    )
}

export default SinglePage

import React, { useEffect, useState } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useUpdatePostMutation } from "../../../apis/postApi"
import { toastNotify } from "../../../helper"
import { CKEditor } from "@ckeditor/ckeditor5-react"
import {
    AccessibilityHelp,
    Autoformat,
    AutoImage,
    Autosave,
    Base64UploadAdapter,
    BlockQuote,
    Bold,
    ClassicEditor,
    Essentials,
    Heading,
    ImageBlock,
    ImageCaption,
    ImageInline,
    ImageInsert,
    ImageInsertViaUrl,
    ImageResize,
    ImageStyle,
    ImageTextAlternative,
    ImageToolbar,
    ImageUpload,
    Indent,
    IndentBlock,
    Italic,
    Link,
    LinkImage,
    Paragraph,
    SelectAll,
    Table,
    TableCaption,
    TableCellProperties,
    TableColumnResize,
    TableProperties,
    TableToolbar,
    TextTransformation,
    Underline,
    Undo,
} from "ckeditor5"
import "ckeditor5/ckeditor5.css"
import { SD_PostStatus, SD_PostType } from "../../../utility/SD"
import { useLocation, useNavigate } from "react-router-dom"

interface PostDto {
    id: number
    title: string
    content: string
    image: string | File | null
    postType: SD_PostType
    status: SD_PostStatus
    userCreated: string
    userUpdated: string
    dateCreated: string
    dateModified: string
}

const postSchema = z.object({
    id: z.number(),
    title: z.string().trim().min(1, "Tiêu đề là bắt buộc").max(255, "Tiêu đề không được vượt quá 255 ký tự"),
    content: z.string().trim().min(1, "Nội dung là bắt buộc"),
    image: z.any().optional(),
    postType: z.nativeEnum(SD_PostType),
    status: z.nativeEnum(SD_PostStatus),
    userCreated: z.string(),
    userUpdated: z.string(),
    dateCreated: z.string(),
    dateModified: z.string(),
})

interface PostInfoProps {
    post: PostDto
}

const PostInfo: React.FC<PostInfoProps> = ({ post }) => {
    const location = useLocation()

    const [isEditing, setIsEditing] = useState(false)
    useEffect(() => {
        // Kiểm tra nếu URL có tham số `edit=1`
        const searchParams = new URLSearchParams(location.search)
        const editParam = searchParams.get("edit")
        setIsEditing(editParam === "1")
    }, [location.search])
    const [content, setContent] = useState(post.content || "")
    const [image, setImage] = useState<string | File | null>(post.image || null)
    const [updatePost] = useUpdatePostMutation()
    const navigate = useNavigate()

    const {
        register,
        handleSubmit,
        control,
        formState: { errors },
        reset,
    } = useForm<PostDto>({
        resolver: zodResolver(postSchema),
        defaultValues: {
            ...post,
            id: post.id,
            postType: Number(post.postType),
            status: Number(post.status),
            dateCreated: post.dateCreated.split("T")[0],
            dateModified: post.dateModified.split("T")[0],
        },
    })

    const onSubmit = async (data: PostDto) => {
        try {
            const formData = new FormData()
            formData.append("id", data.id.toString())
            formData.append("title", data.title)
            formData.append("content", content)
            formData.append("postType", data.postType.toString())
            formData.append("status", data.status.toString())
            formData.append("userCreated", data.userCreated)
            formData.append("userUpdated", data.userUpdated)
            formData.append("dateCreated", data.dateCreated)
            formData.append("dateModified", data.dateModified)

            if (image) {
                formData.append("image", image)
            } else if (post.image) {
                formData.append("image", post.image)
            }

            const response = await updatePost({ id: data.id, postUpdateDto: formData }).unwrap()
            if (response.isSuccess) {
                toastNotify("Cập nhật thông tin bài đăng thành công!", "success")
                setIsEditing(false)
                navigate(`/post/${data.id}`)
            } else {
                toastNotify("Cập nhật thông tin bài đăng thất bại!", "error")
            }
        } catch (err) {
            console.error(err)
            toastNotify("Đã xảy ra lỗi khi cập nhật thông tin bài đăng", "error")
        }
    }

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0] || null
        setImage(file)
    }

    useEffect(() => {
        reset({
            ...post,
            postType: Number(post.postType),
            status: Number(post.status),
            dateCreated: post.dateCreated.split("T")[0],
            dateModified: post.dateModified.split("T")[0],
        })
    }, [post, reset])

    const editorConfig = {
        toolbar: {
            items: [
                "undo",
                "redo",
                "|",
                "heading",
                "|",
                "bold",
                "italic",
                "underline",
                "|",
                "link",
                "insertImage",
                "insertTable",
                "blockQuote",
                "|",
                "outdent",
                "indent",
            ],
            shouldNotGroupWhenFull: false,
        },
        plugins: [
            AccessibilityHelp,
            Autoformat,
            AutoImage,
            Autosave,
            Base64UploadAdapter,
            BlockQuote,
            Bold,
            Essentials,
            Heading,
            ImageBlock,
            ImageCaption,
            ImageInline,
            ImageInsert,
            ImageInsertViaUrl,
            ImageResize,
            ImageStyle,
            ImageTextAlternative,
            ImageToolbar,
            ImageUpload,
            Indent,
            IndentBlock,
            Italic,
            Link,
            LinkImage,
            Paragraph,
            SelectAll,
            Table,
            TableCaption,
            TableCellProperties,
            TableColumnResize,
            TableProperties,
            TableToolbar,
            TextTransformation,
            Underline,
            Undo,
        ],
        heading: {
            options: [
                { model: "paragraph", title: "Paragraph", class: "ck-heading_paragraph" },
                { model: "heading1", view: "h1", title: "Heading 1", class: "ck-heading_heading1" },
                { model: "heading2", view: "h2", title: "Heading 2", class: "ck-heading_heading2" },
                { model: "heading3", view: "h3", title: "Heading 3", class: "ck-heading_heading3" },
            ],
        },
        image: {
            toolbar: [
                "toggleImageCaption",
                "imageTextAlternative",
                "|",
                "imageStyle:inline",
                "imageStyle:wrapText",
                "imageStyle:breakText",
                "|",
                "resizeImage",
            ],
        },
        placeholder: "Type or paste your content here!",
        table: {
            contentToolbar: ["tableColumn", "tableRow", "mergeTableCells", "tableProperties", "tableCellProperties"],
        },
    }

    return (
        <form onSubmit={handleSubmit(onSubmit)} className="row g-3">
            <div className="col-md-6">
                <div>
                    <label htmlFor="image" className="form-label">
                        Ảnh cover
                    </label>
                </div>
                {isEditing ? (
                    <>
                        <input
                            type="file"
                            accept="image/*"
                            onChange={handleImageChange}
                            className="form-control mb-3"
                        />
                        {(image || post.image) && (
                            <div className="mt-2">
                                <img
                                    src={typeof image === "string" ? image : URL.createObjectURL(image as File)}
                                    alt="Cover Preview"
                                    className="img-thumbnail"
                                    style={{ maxWidth: "100%", height: "auto" }}
                                />
                            </div>
                        )}
                    </>
                ) : (
                    post.image && (
                        <img
                            src={post.image as string}
                            alt="Cover"
                            className="img-thumbnail"
                            style={{ maxWidth: "100%", height: "auto" }}
                        />
                    )
                )}
            </div>

            <div className="col-md-6">
                <div className="row">
                    <div className="col-md-6">
                        <label htmlFor="userCreated" className="form-label">
                            Người tạo
                        </label>
                        <input {...register("userCreated")} className="form-control" disabled />
                    </div>
                    <div className="col-md-6">
                        <label htmlFor="dateCreated" className="form-label">
                            Ngày tạo
                        </label>
                        <input type="date" {...register("dateCreated")} className="form-control" disabled />
                    </div>
                    <div className="col-md-6 mt-3">
                        <label htmlFor="userUpdated" className="form-label">
                            Người sửa
                        </label>
                        <input {...register("userUpdated")} className="form-control" disabled />
                    </div>
                    <div className="col-md-6 mt-3">
                        <label htmlFor="dateModified" className="form-label">
                            Ngày sửa
                        </label>
                        <input type="date" {...register("dateModified")} className="form-control" disabled />
                    </div>

                    <div className="col-md-6 mt-3">
                        <label htmlFor="postType" className="form-label">
                            Chọn mục
                        </label>
                        <select
                            {...register("postType", { valueAsNumber: true })}
                            className={`form-select ${errors.postType ? "is-invalid" : ""}`}
                            disabled={!isEditing}
                        >
                            <option value={SD_PostType.Introduction}>Bài đăng giới thiệu</option>
                            <option value={SD_PostType.Activities}>Hoạt động</option>
                            <option value={SD_PostType.Announcement}>Thông báo</option>
                        </select>
                        {errors.postType && <div className="invalid-feedback">{errors.postType.message}</div>}
                    </div>

                    <div className="col-md-6 mt-3">
                        <label htmlFor="status" className="form-label">
                            Trạng thái
                        </label>
                        <select
                            {...register("status", { valueAsNumber: true })}
                            className={`form-select ${errors.status ? "is-invalid" : ""}`}
                            disabled={!isEditing}
                        >
                            <option value={SD_PostStatus.Draft}>Bản nháp</option>
                            <option value={SD_PostStatus.Active}>Hiển thị</option>
                        </select>
                        {errors.status && <div className="invalid-feedback">{errors.status.message}</div>}
                    </div>

                    <div className="col-md-12 mt-3">
                        <label htmlFor="title" className="form-label">
                            Tiêu đề
                        </label>
                        <input
                            {...register("title")}
                            className={`form-control ${errors.title ? "is-invalid" : ""}`}
                            disabled={!isEditing}
                        />
                        {errors.title && <div className="invalid-feedback">{errors.title.message}</div>}
                    </div>
                </div>
            </div>

            <div className="col-md-12">
                <label htmlFor="content" className="form-label">
                    Nội dung
                </label>
                {isEditing ? (
                    <Controller
                        name="content"
                        control={control}
                        render={({ field }) => (
                            <CKEditor
                                editor={ClassicEditor} //@ts-ignore
                                config={editorConfig}
                                data={field.value || ""}
                                onChange={(_, editor) => {
                                    const data = editor.getData()
                                    field.onChange(data)
                                    setContent(data)
                                }}
                                onBlur={field.onBlur}
                            />
                        )}
                    />
                ) : (
                    <div dangerouslySetInnerHTML={{ __html: content }} />
                )}
                {errors.content && <div className="invalid-feedback">{errors.content.message}</div>}
            </div>

            <div className="col-12 text-end">
                <button type="button" className="btn btn-outline-secondary me-3" onClick={() => navigate(-1)}>
                    Quay lại
                </button>
                {isEditing ? (
                    <>
                        <button type="button" className="btn btn-secondary me-3" onClick={() => setIsEditing(false)}>
                            Hủy
                        </button>
                        <button type="submit" className="btn btn-primary">
                            Cập nhật
                        </button>
                    </>
                ) : (
                    <button type="button" className="btn btn-primary" onClick={() => setIsEditing(true)}>
                        Chỉnh sửa bài đăng
                    </button>
                )}
            </div>
        </form>
    )
}

export default PostInfo

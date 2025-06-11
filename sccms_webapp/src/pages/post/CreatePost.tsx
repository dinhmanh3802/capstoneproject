import React, { useState } from "react"
import { useForm, Controller } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
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
import { useCreatePostMutation } from "../../apis/postApi"
import { toastNotify } from "../../helper"
import { useNavigate } from "react-router-dom"
import { SD_PostType } from "../../utility/SD"

const postSchema = z.object({
    title: z.string().trim().min(1, "Tiêu đề là bắt buộc").max(255, "Tiêu đề không được vượt quá 255 ký tự"),
    content: z.string().trim().min(1, "Nội dung là bắt buộc"),
    image: z
        .instanceof(FileList, { message: "Ảnh cover là bắt buộc" })
        .refine((files) => files.length > 0, "Ảnh cover là bắt buộc")
        .transform((files) => files[0]),
    postType: z.enum(
        [SD_PostType.Introduction.toString(), SD_PostType.Activities.toString(), SD_PostType.Announcement.toString()],
        {
            errorMap: () => ({ message: "Chọn một mục hợp lệ" }),
        },
    ),
})

type PostFormData = z.infer<typeof postSchema>

const CreatePost: React.FC = () => {
    const navigate = useNavigate()
    const [createPost] = useCreatePostMutation()
    const [imagePreview, setImagePreview] = useState<string | null>(null)
    const [isCreatePostLoading, setIsCreatePostLoading] = useState(false)

    const {
        register,
        handleSubmit,
        control,
        formState: { errors },
    } = useForm<PostFormData>({
        resolver: zodResolver(postSchema),
        defaultValues: {
            title: "",
            content: "",
            postType: SD_PostType.Introduction.toString(),
        },
    })

    const onSubmit = async (data: PostFormData) => {
        setIsCreatePostLoading(true)
        try {
            const formData = new FormData()
            formData.append("title", data.title)
            formData.append("content", data.content)
            formData.append("postType", data.postType.toString())
            formData.append("image", data.image)

            const response = await createPost(formData).unwrap()
            if (response.isSuccess) {
                toastNotify("Tạo bài đăng thành công!", "success")
                navigate("/post")
            } else {
                toastNotify("Tạo bài đăng thất bại!", "error")
            }
        } catch (err) {
            console.error(err)
            toastNotify("Đã xảy ra lỗi khi tạo bài đăng", "error")
        }
        setIsCreatePostLoading(false)
    }

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = e.target.files
        if (files && files.length > 0) {
            const file = files[0]
            const objectUrl = URL.createObjectURL(file)
            setImagePreview(objectUrl)
            return () => URL.revokeObjectURL(objectUrl)
        } else {
            setImagePreview(null)
        }
    }

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
        <div className="container">
            <div className="mt-4 mb-3">
                <h3 className="fw-bold primary-color">Tạo Bài đăng Mới</h3>
            </div>
            <form onSubmit={handleSubmit(onSubmit)} className="row g-4">
                {/* Các trường khác */}
                {/* Hàng 1: Ảnh cover */}
                <div className="col-12">
                    <label htmlFor="image" className="form-label">
                        Ảnh cover <span style={{ color: "red" }}>*</span>
                    </label>
                    <Controller
                        name="image"
                        control={control}
                        render={({ field }) => (
                            <input
                                type="file"
                                accept="image/*"
                                onChange={(e) => {
                                    field.onChange(e.target.files)
                                    handleImageChange(e)
                                }}
                                className={`form-control ${errors.image ? "is-invalid" : ""}`}
                            />
                        )}
                    />
                    {errors.image && <div className="invalid-feedback">{errors.image.message}</div>}
                    {imagePreview && (
                        <div className="mt-3">
                            <img
                                src={imagePreview}
                                alt="Cover Preview"
                                className="img-thumbnail"
                                style={{ maxWidth: "100%", height: "auto" }}
                            />
                        </div>
                    )}
                </div>

                {/* Hàng 2: Chọn mục và Tiêu đề */}
                <div className="col-12">
                    <div className="row g-3">
                        <div className="col-md-6">
                            <label htmlFor="postType" className="form-label">
                                Chọn mục <span style={{ color: "red" }}>*</span>
                            </label>
                            <Controller
                                name="postType"
                                control={control}
                                render={({ field }) => (
                                    <select {...field} className={`form-select ${errors.postType ? "is-invalid" : ""}`}>
                                        <option value={SD_PostType.Introduction}>Bài đăng giới thiệu</option>
                                        <option value={SD_PostType.Activities}>Hoạt động</option>
                                        <option value={SD_PostType.Announcement}>Thông báo</option>
                                    </select>
                                )}
                            />
                            {errors.postType && <div className="invalid-feedback">{errors.postType.message}</div>}
                        </div>

                        <div className="col-md-6">
                            <label htmlFor="title" className="form-label">
                                Tiêu đề <span style={{ color: "red" }}>*</span>
                            </label>
                            <input
                                {...register("title")}
                                className={`form-control ${errors.title ? "is-invalid" : ""}`}
                                placeholder="Nhập tiêu đề bài đăng"
                            />
                            {errors.title && <div className="invalid-feedback">{errors.title.message}</div>}
                        </div>
                    </div>
                </div>

                <div className="col-12">
                    <label htmlFor="content" className="form-label">
                        Nội dung <span style={{ color: "red" }}>*</span>
                    </label>
                    <Controller
                        name="content"
                        control={control}
                        render={({ field }) => (
                            <CKEditor
                                editor={ClassicEditor} // @ts-ignore
                                config={editorConfig}
                                data={field.value || ""}
                                onChange={(_, editor) => {
                                    const data = editor.getData()
                                    field.onChange(data)
                                }}
                                onBlur={field.onBlur}
                            />
                        )}
                    />
                    {errors.content && <div className="invalid-feedback">{errors.content.message}</div>}
                </div>

                {/* Nút hành động */}
                <div className="col-12 text-end">
                    <button type="button" className="btn btn-outline-secondary me-3" onClick={() => navigate(-1)}>
                        Quay lại
                    </button>
                    <button type="submit" className="btn btn-primary" disabled={isCreatePostLoading}>
                        Tạo bài đăng
                    </button>
                </div>
            </form>
        </div>
    )
}

export default CreatePost

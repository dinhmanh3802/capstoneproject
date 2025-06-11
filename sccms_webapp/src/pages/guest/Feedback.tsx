import { useState, useEffect } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import ReCAPTCHA from "react-google-recaptcha"
import { toast } from "react-toastify"
import { useGetCourseQuery, useGetFeedbackCourseQuery } from "../../apis/courseApi"
import { useCreateFeedbackMutation } from "../../apis/feedbackApi"
import { useNavigate } from "react-router-dom"

const formSchema = z.object({
    studentCode: z
        .string()
        .trim()
        .min(1, { message: "Mã khóa sinh là bắt buộc" })
        .length(9, { message: "Mã khóa sinh phải có 9 ký tự" }),
    courseId: z.string().min(1, { message: "Vui lòng chọn khóa tu" }),
    content: z
        .string()
        .trim()
        .min(1, { message: "Phản hồi không được để trống" })
        .max(10000, { message: "Phản hồi không được vượt quá 10000 ký tự" }),
})

type FormData = z.infer<typeof formSchema>

function Feedback() {
    const [captchaToken, setCaptchaToken] = useState<string | null>(null)
    const [courseList, setCourseList] = useState([])
    const { data: courses } = useGetFeedbackCourseQuery({})
    const [createFeedback] = useCreateFeedbackMutation()
    const navigate = useNavigate()

    const {
        register,
        handleSubmit,
        formState: { errors },
        setError,
    } = useForm<FormData>({
        resolver: zodResolver(formSchema),
    })

    useEffect(() => {
        if (courses) {
            setCourseList(courses.result)
        }
    }, [courses])

    const handleCaptcha = (token: string | null) => {
        setCaptchaToken(token)
    }

    const onSubmit = async (data: FormData) => {
        if (!captchaToken) {
            toast.error("Vui lòng xác nhận CAPTCHA")
            return
        }

        try {
            const response = await createFeedback({
                studentCode: data.studentCode,
                courseId: data.courseId,
                content: data.content,
            }).unwrap()
            if (response.isSuccess) {
                toast.success("Gửi phản hồi thành công")
                navigate("/home/feedbacksuccess")
            }
        } catch (error: any) {
            const errorMessage = error.data.errorMessages?.find(
                (msg: any) => msg === "Mã khóa sinh không tồn tại." || msg === "Khóa sinh không tham gia khóa tu này.",
            )
            if (errorMessage) {
                setError("studentCode", {
                    type: "manual",
                    message: errorMessage,
                })
            } else {
                toast.error(error.errorMessages.join(", ") || "Lỗi không xác định")
            }
        }
    }

    return (
        <div
            className="container p-5 rounded"
            style={{ marginTop: "20px", maxWidth: "600px", backgroundColor: "white" }}
        >
            <h3 className="text-center mb-4">Gửi phản hồi</h3>
            <form onSubmit={handleSubmit(onSubmit)} className="row g-3 mx-auto">
                {/* Mã khóa sinh */}
                <div className="col-12">
                    <label className="form-label">Nhập mã khóa sinh</label>
                    <input
                        {...register("studentCode")}
                        className={`form-control ${errors.studentCode ? "is-invalid" : ""}`}
                        type="text"
                        placeholder="Nhập mã khóa sinh"
                    />
                    {errors.studentCode && <div className="invalid-feedback">{errors.studentCode.message}</div>}
                </div>

                {/* Chọn khóa tu */}
                <div className="col-12">
                    <label className="form-label">Chọn khóa tu</label>
                    <select {...register("courseId")} className={`form-select ${errors.courseId ? "is-invalid" : ""}`}>
                        <option value="">Chọn khóa tu</option>
                        {courseList?.map((item: any) => (
                            <option key={item.id} value={item.id}>
                                {item.courseName}
                            </option>
                        ))}
                    </select>
                    {errors.courseId && <div className="invalid-feedback">{errors.courseId.message}</div>}
                </div>

                {/* Phản hồi */}
                <div className="col-12">
                    <label className="form-label">Phản hồi</label>
                    <textarea
                        {...register("content")}
                        className={`form-control ${errors.content ? "is-invalid" : ""}`}
                        placeholder="Nhập phản hồi"
                        rows={8}
                    ></textarea>
                    {errors.content && <div className="invalid-feedback">{errors.content.message}</div>}
                </div>

                {/* reCAPTCHA */}
                <div className="col-12">
                    <ReCAPTCHA sitekey="6Le5q2kqAAAAAPNSdBXL3sX_zQHcjy9xFrXsk-mL" onChange={handleCaptcha} />
                </div>

                {/* Nút gửi */}
                <div className="col-12 text-end">
                    <button type="submit" className="btn btn-primary">
                        Gửi
                    </button>
                </div>
            </form>
        </div>
    )
}

export default Feedback

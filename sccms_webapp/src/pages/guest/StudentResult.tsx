import { useState, useEffect } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import ReCAPTCHA from "react-google-recaptcha"
import { toast } from "react-toastify"
import { useGetCourseQuery } from "../../apis/courseApi"
import { useGetReportsByStudentQuery } from "../../apis/reportApi"
import { StudentReportViewDto } from "../../interfaces"
import { MainLoader } from "../../components/Page"
import { parseISO, format } from "date-fns"

// Define the form schema using Zod
const formSchema = z.object({
    studentCode: z.string().trim().min(1, { message: "Mã khóa sinh là bắt buộc" }),
    // .length(9, { message: "Mã khóa sinh phải có 9 ký tự" }), // Uncomment if needed
    courseId: z.string().trim().min(1, { message: "Vui lòng chọn khóa tu" }),
})

// Infer the form data type from the schema
type FormData = z.infer<typeof formSchema>

function StudentResult() {
    // State for CAPTCHA token
    const [captchaToken, setCaptchaToken] = useState<string | null>(null)

    // State for the list of courses
    const [courseList, setCourseList] = useState<any[]>([])

    // State to trigger data fetching
    const [triggerFetch, setTriggerFetch] = useState<boolean>(false)

    // Fetch courses with a specific status (adjust the status as needed)
    const { data: courses, isLoading: isCoursesLoading, error: coursesError } = useGetCourseQuery({})

    // React Hook Form setup with Zod resolver for validation
    const {
        register,
        handleSubmit,
        formState: { errors },
        setError,
    } = useForm<FormData>({
        resolver: zodResolver(formSchema),
    })

    // Effect to set the course list once courses are fetched
    useEffect(() => {
        if (courses) {
            const filteredCourses = courses.result.filter((course) => course.status != 0)
            setCourseList(filteredCourses)
        }
    }, [courses])

    // Handle CAPTCHA verification
    const handleCaptcha = (token: string | null) => {
        setCaptchaToken(token)
    }

    // State to store API parameters for fetching reports
    const [apiParams, setApiParams] = useState<{ studentCode: string; courseId: number } | null>(null)

    // Use the API hook with conditional fetching based on apiParams
    const { data, error, isLoading } = useGetReportsByStudentQuery(apiParams!, {
        skip: !apiParams,
    })

    // Handle form submission
    const onSubmit = (data: FormData) => {
        // Uncomment the CAPTCHA validation after enabling CAPTCHA in the form
        /*
        if (!captchaToken) {
            toast.error("Vui lòng xác nhận CAPTCHA");
            return;
        }
        */

        // Parse courseId to number
        const courseIdNumber = parseInt(data.courseId, 10)

        // Set API parameters to trigger fetch
        setApiParams({
            studentCode: data.studentCode,
            courseId: courseIdNumber,
        })

        setTriggerFetch(true)
    }

    // Process and group reports by date, concatenating comments
    const getGroupedReports = () => {
        if (!data?.result || data.result.length === 0) return []

        // Reduce the reports to group by date
        const grouped: { [key: string]: string[] } = data.result.reduce((acc, report) => {
            const dateKey = format(parseISO(report.date), "dd/MM/yyyy")
            if (!acc[dateKey]) {
                acc[dateKey] = []
            }
            if (report.content && report.content.trim() !== "") {
                acc[dateKey].push(report.content)
            }
            return acc
        }, {} as { [key: string]: string[] })

        // Convert the grouped object to an array for rendering, excluding empty comment groups
        return Object.entries(grouped)
            .filter(([date, comments]) => comments.length > 0)
            .map(([date, comments]) => ({
                date,
                comments: comments.join("\n"), // Join comments with line breaks
            }))
    }

    // Retrieve the grouped reports
    const groupedReports = getGroupedReports()

    return (
        <div
            className="container p-5 rounded"
            style={{ marginTop: "20px", maxWidth: "900px", minHeight: "80vh", backgroundColor: "white" }}
        >
            <h3 className="text-center mb-4">Tra cứu đánh giá khóa sinh</h3>
            <form onSubmit={handleSubmit(onSubmit)} className="row g-3 mx-auto border rounded-2 p-2 border-black">
                {/* Mã khóa sinh */}
                <div className="col-6">
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
                <div className="col-6">
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

                {/* reCAPTCHA */}
                {/* Uncomment and replace YOUR_SITE_KEY with your actual site key after setting up reCAPTCHA
                <div className="col-12">
                    <ReCAPTCHA sitekey="YOUR_SITE_KEY" onChange={handleCaptcha} />
                </div>
                */}

                {/* Nút gửi */}
                <div className="col-12 text-end">
                    <button type="submit" className="btn btn-primary">
                        Tra cứu
                    </button>
                </div>
            </form>

            {/* Kết quả */}
            <div className="mt-5">
                {/* Loading State */}
                {isLoading && <MainLoader />}

                {/* Error Handling */}
                {error && (
                    <div className="alert alert-danger">
                        <p>Không tìm thấy đánh giá</p>
                    </div>
                )}

                {/* Display Grouped Reports */}
                {groupedReports.length > 0 ? (
                    <div>
                        <h4 className="text-center">Kết quả</h4>
                        <table className="table table-bordered">
                            <thead>
                                <tr>
                                    <th>Ngày</th>
                                    <th>Nhận xét</th>
                                </tr>
                            </thead>
                            <tbody>
                                {groupedReports.map((report, index) => {
                                    return (
                                        <tr key={index}>
                                            <td>{report.date}</td>
                                            <td style={{ whiteSpace: "pre-line" }}>{report.comments}</td>
                                        </tr>
                                    )
                                })}
                            </tbody>
                        </table>
                    </div>
                ) : (
                    triggerFetch && !isLoading && !error && <p>Không tìm thấy báo cáo nào.</p>
                )}
            </div>
        </div>
    )
}

export default StudentResult

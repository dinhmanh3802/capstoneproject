import { useState, useEffect, useCallback, useRef } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useNavigate } from "react-router-dom"
import { error } from "../../utility/Message"
import ReCAPTCHA from "react-google-recaptcha"
import { toast } from "react-toastify"
import { useGetCourseQuery } from "../../apis/courseApi"
import { useCreateStudentMutation } from "../../apis/studentApi"
import Cropper from "react-easy-crop"
import Modal from "react-bootstrap/Modal"
import Button from "react-bootstrap/Button"
import imageCompression from "browser-image-compression"
import { Image, Row, Col, Form, Card } from "react-bootstrap"
import { MainLoader } from "../../components/Page"

// Các hằng số
const MAX_FILE_SIZE = 5000000 // 5MB
const MAX_OVERALL_FILE_SIZE = 20000000 // 20MB
const ACCEPTED_IMAGE_TYPES = ["image/jpeg", "image/jpg", "image/png"]

// Schema cho form đăng ký sinh viên
const formSchema = z.object({
    courseId: z.string().min(1, { message: error.courseNameRequired }),
    fullName: z
        .string()
        .trim()
        .min(1, { message: `${error.fullNameRequired}` })
        .max(100, { message: `${error.fullNameTooLong}` })
        .regex(/^[A-Za-zÀ-Ỹà-ỹ\s]+$/, { message: `${error.invalidFullName}` })
        .refine((value) => !/\s{2,}/.test(value), { message: `${error.noConsecutiveSpaces}` })
        .refine((value) => value.trim() === value, { message: `${error.noLeadingOrTrailingSpaces}` }),

    gender: z.string().min(1, { message: `${error.genderRequired}` }),
    dateOfBirth: z
        .string()
        .min(1, { message: error.dateOfBirthRequired })
        .refine(
            (date) => {
                const parsedDate = Date.parse(date)
                if (isNaN(parsedDate)) {
                    return false
                }
                const birthYear = new Date(parsedDate).getFullYear()
                const currentYear = new Date().getFullYear()
                const age = currentYear - birthYear
                return age >= 8 && age <= 17
            },
            { message: error.invalidAgeRange },
        ),
    idNumber: z.string().regex(/^\d{9,12}$/, { message: `${error.idNumberLengthError}` }),
    email: z.string().email({ message: `${error.invalidEmail}` }),
    address: z
        .string()
        .trim()
        .min(1, { message: `${error.addressRequired}` }),
    parentName: z
        .string()
        .trim()
        .min(1, { message: `${error.parentNameRequired}` })
        .max(100, { message: `${error.parentNameTooLong}` })
        .regex(/^[A-Za-zÀ-Ỹà-ỹ\s]+$/, { message: `${error.invalidParentName}` })
        .refine((value) => !/\s{2,}/.test(value), { message: `${error.noConsecutiveSpacesParentName}` })
        .refine((value) => value.trim() === value, { message: `${error.noLeadingOrTrailingSpacesParentName}` }),

    parentPhone: z
        .string()
        .regex(/^(0[3|5|7|8|9])+([0-9]{8})$/, { message: `${error.invalidPhone}` })
        .min(10, { message: `${error.phoneRequired}` })
        .max(10, { message: `${error.phoneTooLong}` }),

    photo: z
        .any()
        .refine((files) => files?.length === 1, { message: `${error.photoRequired}` })
        .refine((files) => files?.[0]?.size <= MAX_FILE_SIZE, { message: `${error.photoMaxSize}` })
        .refine((files) => ACCEPTED_IMAGE_TYPES.includes(files?.[0]?.type), { message: `${error.photoInvalidType}` })
        .refine((files) => files?.[0]?.size <= MAX_OVERALL_FILE_SIZE, {
            message: "Ảnh thẻ quá lớn. Vui lòng chọn ảnh dưới 20MB.",
        }),

    frontIdCard: z
        .any()
        .refine((files) => files?.length === 1, { message: `${error.frontIdCardRequired}` })
        .refine((files) => files?.[0]?.size <= MAX_FILE_SIZE, { message: `${error.frontIdCardMaxSize}` })
        .refine((files) => ACCEPTED_IMAGE_TYPES.includes(files?.[0]?.type), {
            message: `${error.frontIdCardInvalidType}`,
        })
        .refine((files) => files?.[0]?.size <= MAX_OVERALL_FILE_SIZE, {
            message: "Ảnh mặt trước CCCD quá lớn. Vui lòng chọn ảnh dưới 20MB.",
        }),

    backIdCard: z
        .any()
        .refine((files) => files?.length === 1, { message: `${error.backIdCardRequired}` })
        .refine((files) => files?.[0]?.size <= MAX_FILE_SIZE, { message: `${error.backIdCardMaxSize}` })
        .refine((files) => ACCEPTED_IMAGE_TYPES.includes(files?.[0]?.type), {
            message: `${error.backIdCardInvalidType}`,
        })
        .refine((files) => files?.[0]?.size <= MAX_OVERALL_FILE_SIZE, {
            message: "Ảnh mặt sau CCCD quá lớn. Vui lòng chọn ảnh dưới 20MB.",
        }),
    academicPerformance: z
        .string()
        .trim()
        .min(1, { message: `${error.academicPerformanceRequired}` }),
    conduct: z
        .string()
        .trim()
        .min(1, { message: `${error.conductRequired}` }),
})

type FormData = z.infer<typeof formSchema>

function StudentRegistration() {
    const [loading, setLoading] = useState(false)
    const navigate = useNavigate()
    const [captchaToken, setCaptchaToken] = useState<string | null>(null)

    const handleCaptcha = (token: string | null) => {
        setCaptchaToken(token)
    }

    const {
        register,
        handleSubmit,
        setValue,
        trigger,
        formState: { errors },
    } = useForm<FormData>({
        resolver: zodResolver(formSchema),
    })

    const [courseList, setCourseList] = useState<any[]>([])
    const { data: courses } = useGetCourseQuery({ status: 1 })
    useEffect(() => {
        if (courses) {
            setCourseList(courses.result)
        }
    }, [courses])

    const [createStudent] = useCreateStudentMutation()

    // States for cropping
    const [crop, setCrop] = useState<{ x: number; y: number }>({ x: 0, y: 0 })
    const [zoom, setZoom] = useState<number>(1)
    const [croppedAreaPixels, setCroppedAreaPixels] = useState<any>(null)
    const [croppingImage, setCroppingImage] = useState<{
        field: keyof FormData
        file: File
        imageSrc: string
    } | null>(null)
    const [showCropModal, setShowCropModal] = useState<boolean>(false)
    const [cropAspect, setCropAspect] = useState<number>(2 / 3) // Default aspect ratio for photo

    // Refs to reset input fields
    const photoInputRef = useRef<HTMLInputElement | null>(null)
    const frontIdCardInputRef = useRef<HTMLInputElement | null>(null)
    const backIdCardInputRef = useRef<HTMLInputElement | null>(null)

    // States to store file previews
    const [photoPreview, setPhotoPreview] = useState<string | null>(null)
    const [frontIdCardPreview, setFrontIdCardPreview] = useState<string | null>(null)
    const [backIdCardPreview, setBackIdCardPreview] = useState<string | null>(null)

    const onCropComplete = useCallback((croppedArea: any, croppedAreaPixels: any) => {
        setCroppedAreaPixels(croppedAreaPixels)
    }, [])

    const createImage = (url: string): Promise<HTMLImageElement> =>
        new Promise((resolve, reject) => {
            const image = document.createElement("img") // Sử dụng createElement thay vì 'new Image()'
            image.addEventListener("load", () => resolve(image))
            image.addEventListener("error", (error) => reject(error))
            image.setAttribute("crossOrigin", "anonymous") // Tránh lỗi Cross-Origin khi dùng hình ảnh ngoài
            image.src = url
        })

    const getCroppedImg = async (imageSrc: string, pixelCrop: any): Promise<Blob | null> => {
        const image = await createImage(imageSrc)
        const canvas = document.createElement("canvas")
        const ctx = canvas.getContext("2d")

        if (!ctx) {
            return null
        }

        canvas.width = pixelCrop.width
        canvas.height = pixelCrop.height

        ctx.drawImage(
            image,
            pixelCrop.x,
            pixelCrop.y,
            pixelCrop.width,
            pixelCrop.height,
            0,
            0,
            pixelCrop.width,
            pixelCrop.height,
        )

        return new Promise((resolve, reject) => {
            canvas.toBlob((blob) => {
                if (!blob) {
                    console.error("Canvas is empty")
                    return
                }
                resolve(blob)
            }, "image/jpeg")
        })
    }

    const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>, field: keyof FormData) => {
        if (e.target.files && e.target.files.length > 0) {
            const file = e.target.files[0]
            const fileSize = file.size

            // Kiểm tra kích thước tối đa 20MB
            if (fileSize > MAX_OVERALL_FILE_SIZE) {
                toast.error("Ảnh quá lớn. Vui lòng chọn ảnh dưới 20MB.")
                // Reset input field
                if (field === "photo" && photoInputRef.current) {
                    photoInputRef.current.value = ""
                } else if (field === "frontIdCard" && frontIdCardInputRef.current) {
                    frontIdCardInputRef.current.value = ""
                } else if (field === "backIdCard" && backIdCardInputRef.current) {
                    backIdCardInputRef.current.value = ""
                }
                return
            }

            // Kiểm tra kích thước tối đa hiện tại 5MB
            if (fileSize > MAX_FILE_SIZE) {
                toast.error("Ảnh quá lớn. Vui lòng chọn ảnh dưới 5MB.")
                // Reset input field
                if (field === "photo" && photoInputRef.current) {
                    photoInputRef.current.value = ""
                } else if (field === "frontIdCard" && frontIdCardInputRef.current) {
                    frontIdCardInputRef.current.value = ""
                } else if (field === "backIdCard" && backIdCardInputRef.current) {
                    backIdCardInputRef.current.value = ""
                }
                return
            }

            const fileURL = URL.createObjectURL(file)

            // Set aspect ratio based on field
            if (field === "photo") {
                setCropAspect(1 / 1) // 1x1
            } else if (field === "frontIdCard" || field === "backIdCard") {
                setCropAspect(43 / 27) // 54x86
            }

            setCroppingImage({ field, file, imageSrc: fileURL })
            setShowCropModal(true)
        }
    }

    const handleCropSave = async () => {
        if (croppingImage && croppedAreaPixels) {
            const croppedBlob = await getCroppedImg(croppingImage.imageSrc, croppedAreaPixels)
            if (croppedBlob) {
                // Compress the image if it's larger than MAX_FILE_SIZE
                let compressedFile: File = new File([croppedBlob], croppingImage.file.name, {
                    type: "image/jpeg",
                })

                if (compressedFile.size > MAX_FILE_SIZE) {
                    try {
                        const options = {
                            maxSizeMB: 5,
                            maxWidthOrHeight: 1920,
                            useWebWorker: true,
                        }
                        const compressedBlob = await imageCompression(compressedFile, options)
                        compressedFile = new File([compressedBlob], croppingImage.file.name, {
                            type: compressedBlob.type,
                        })
                    } catch (error) {
                        console.error("Error during image compression:", error)
                        toast.error("Lỗi khi nén ảnh.")
                        return
                    }
                }

                // Đặt giá trị cho trường tương ứng dưới dạng mảng chứa File
                setValue(croppingImage.field, [compressedFile], { shouldValidate: true })
                await trigger(croppingImage.field) // Trigger validation for the specific field

                // Cập nhật preview và reset các state
                const compressedFileURL = URL.createObjectURL(compressedFile)
                if (croppingImage.field === "photo") {
                    setPhotoPreview(compressedFileURL)
                } else if (croppingImage.field === "frontIdCard") {
                    setFrontIdCardPreview(compressedFileURL)
                } else if (croppingImage.field === "backIdCard") {
                    setBackIdCardPreview(compressedFileURL)
                }

                // Reset input field thông qua ref
                if (croppingImage.field === "photo" && photoInputRef.current) {
                    photoInputRef.current.value = ""
                } else if (croppingImage.field === "frontIdCard" && frontIdCardInputRef.current) {
                    frontIdCardInputRef.current.value = ""
                } else if (croppingImage.field === "backIdCard" && backIdCardInputRef.current) {
                    backIdCardInputRef.current.value = ""
                }

                // Reset cropping states
                setCroppingImage(null)
                setCroppedAreaPixels(null)
                setZoom(1)
                setCrop({ x: 0, y: 0 })
                setShowCropModal(false)
            }
        }
    }

    const removeSelectedFile = (field: keyof FormData) => {
        if (field === "photo") {
            setPhotoPreview(null)
            setValue("photo", [], { shouldValidate: true })
        } else if (field === "frontIdCard") {
            setFrontIdCardPreview(null)
            setValue("frontIdCard", [], { shouldValidate: true })
        } else if (field === "backIdCard") {
            setBackIdCardPreview(null)
            setValue("backIdCard", [], { shouldValidate: true })
        }
    }

    const onSubmit = async (data: FormData) => {
        if (!captchaToken) {
            toast.error("Vui lòng xác nhận CAPTCHA")
            return
        }
        setLoading(true)
        try {
            // Kiểm tra các trường ảnh sau khi xử lý không trống
            if (!data.photo || data.photo.length === 0) {
                toast.error("Vui lòng tải lên ảnh thẻ.")
                setLoading(false)
                return
            }
            if (!data.frontIdCard || data.frontIdCard.length === 0) {
                toast.error("Vui lòng tải lên ảnh mặt trước CCCD.")
                setLoading(false)
                return
            }
            if (!data.backIdCard || data.backIdCard.length === 0) {
                toast.error("Vui lòng tải lên ảnh mặt sau CCCD.")
                setLoading(false)
                return
            }

            const response = await createStudent({ courseId: data.courseId, studentData: data }).unwrap()

            if (response.isSuccess) {
                navigate("/home/submitsuccess")
            } else {
                const errorMessage =
                    response.errorMessages.length > 0 ? response.errorMessages.join(", ") : "Lỗi không xác định"
                toast.error(errorMessage)
            }
        } catch (error) {
            toast.error("Có lỗi xảy ra khi đăng ký.")
            console.error(error)
        } finally {
            setLoading(false)
        }
    }

    if (loading)
        return (
            <div style={{ height: "80vh" }}>
                <MainLoader />
            </div>
        )

    return (
        <div className="container p-4 my-5 rounded shadow-sm" style={{ maxWidth: "900px", backgroundColor: "#ffffff" }}>
            <div className="text-center mb-4">
                <h3 className="fw-bold primary-color">Đăng Ký Khóa Sinh</h3>
            </div>
            <Form onSubmit={handleSubmit(onSubmit)}>
                {/* Thông Tin Cá Nhân */}
                <Card className="mb-4">
                    <Card.Header className="bg-primary text-white">Thông Tin Cá Nhân</Card.Header>
                    <Card.Body>
                        <Row className="g-3">
                            {/* Khóa tu */}
                            <Col md={6}>
                                <Form.Group controlId="courseId">
                                    <Form.Label>
                                        Khóa tu <span className="text-danger">*</span>
                                    </Form.Label>
                                    <Form.Select {...register("courseId")} isInvalid={!!errors.courseId}>
                                        <option value="">Chọn khóa tu</option>
                                        {courseList?.map((item: any) => (
                                            <option key={item.id} value={item.id}>
                                                {item.courseName}
                                            </option>
                                        ))}
                                    </Form.Select>
                                    <Form.Control.Feedback type="invalid">
                                        {errors.courseId?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Họ và tên */}
                            <Col md={6}>
                                <Form.Group controlId="fullName">
                                    <Form.Label>
                                        Họ và tên <span className="text-danger">*</span>
                                    </Form.Label>
                                    <Form.Control
                                        type="text"
                                        placeholder="Nhập họ và tên..."
                                        {...register("fullName")}
                                        isInvalid={!!errors.fullName}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.fullName?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Số CMND/CCCD */}
                            <Col md={6}>
                                <Form.Group controlId="idNumber">
                                    <Form.Label>
                                        Số CMND/CCCD <span className="text-danger">*</span>
                                    </Form.Label>
                                    <Form.Control
                                        type="text"
                                        placeholder="Nhập số CMND/CCCD..."
                                        {...register("idNumber")}
                                        isInvalid={!!errors.idNumber}
                                        inputMode="numeric"
                                        pattern="[0-9]*"
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.idNumber?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Giới tính */}
                            <Col md={3}>
                                <Form.Group controlId="gender">
                                    <Form.Label>
                                        Giới tính <span className="text-danger">*</span>
                                    </Form.Label>
                                    <Form.Select {...register("gender")} isInvalid={!!errors.gender}>
                                        <option value="">Chọn giới tính</option>
                                        <option value="0">Nam</option>
                                        <option value="1">Nữ</option>
                                    </Form.Select>
                                    <Form.Control.Feedback type="invalid">
                                        {errors.gender?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Ngày sinh */}
                            <Col md={3}>
                                <Form.Group controlId="dateOfBirth">
                                    <Form.Label>
                                        Ngày sinh <span className="text-danger">*</span>
                                    </Form.Label>
                                    <Form.Control
                                        type="date"
                                        {...register("dateOfBirth")}
                                        isInvalid={!!errors.dateOfBirth}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.dateOfBirth?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Học lực */}
                            <Col md={6}>
                                <Form.Group controlId="academicPerformance">
                                    <Form.Label>
                                        Học lực <span className="text-danger">*</span>
                                    </Form.Label>
                                    <Form.Control
                                        type="text"
                                        placeholder="Nhập học lực..."
                                        {...register("academicPerformance")}
                                        isInvalid={!!errors.academicPerformance}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.academicPerformance?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Hạnh kiểm */}
                            <Col md={6}>
                                <Form.Group controlId="conduct">
                                    <Form.Label>
                                        Hạnh kiểm <span className="text-danger">*</span>
                                    </Form.Label>
                                    <Form.Control
                                        type="text"
                                        placeholder="Nhập hạnh kiểm..."
                                        {...register("conduct")}
                                        isInvalid={!!errors.conduct}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.conduct?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Địa chỉ */}
                            <Col md={6}>
                                <Form.Group controlId="address">
                                    <Form.Label>
                                        Địa chỉ <span className="text-danger">*</span>
                                    </Form.Label>
                                    <Form.Control
                                        type="text"
                                        placeholder="Nhập địa chỉ..."
                                        {...register("address")}
                                        isInvalid={!!errors.address}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.address?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Họ và tên cha/mẹ */}
                            <Col md={6}>
                                <Form.Group controlId="parentName">
                                    <Form.Label>
                                        Họ và tên cha/mẹ <span className="text-danger">*</span>
                                    </Form.Label>
                                    <Form.Control
                                        type="text"
                                        placeholder="Nhập tên cha/mẹ..."
                                        {...register("parentName")}
                                        isInvalid={!!errors.parentName}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.parentName?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Số điện thoại cha/mẹ */}
                            <Col md={6}>
                                <Form.Group controlId="parentPhone">
                                    <Form.Label>
                                        Số điện thoại cha/mẹ <span className="text-danger">*</span>
                                    </Form.Label>
                                    <Form.Control
                                        type="text"
                                        placeholder="Nhập số điện thoại cha/mẹ..."
                                        {...register("parentPhone")}
                                        isInvalid={!!errors.parentPhone}
                                        inputMode="numeric"
                                        pattern="[0-9]*"
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.parentPhone?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Email */}
                            <Col md={6}>
                                <Form.Group controlId="email">
                                    <Form.Label>
                                        Email <span className="text-danger">*</span>
                                    </Form.Label>
                                    <Form.Control
                                        type="email"
                                        placeholder="Nhập địa chỉ email..."
                                        {...register("email")}
                                        isInvalid={!!errors.email}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.email?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>
                        </Row>
                    </Card.Body>
                </Card>

                {/* Tải Lên Ảnh */}
                <Card className="mb-4">
                    <Card.Header className="bg-primary text-white">Tải Lên Ảnh</Card.Header>
                    <Card.Body>
                        <Row className="g-4">
                            {/* Ảnh thẻ */}
                            <Col md={4}>
                                <Form.Group controlId="photo">
                                    <Form.Label>
                                        Ảnh thẻ <span className="text-danger">*</span>
                                    </Form.Label>
                                    <div
                                        className={`d-flex flex-column align-items-center justify-content-center p-3 border rounded ${
                                            errors.photo ? "border-danger" : "border-secondary"
                                        }`}
                                        style={{ height: "200px", position: "relative" }}
                                    >
                                        {photoPreview ? (
                                            <>
                                                <Image src={photoPreview} thumbnail style={{ maxHeight: "150px" }} />
                                                <Button
                                                    variant="danger"
                                                    size="sm"
                                                    className="position-absolute top-0 end-0"
                                                    onClick={() => removeSelectedFile("photo")}
                                                >
                                                    &times;
                                                </Button>
                                            </>
                                        ) : (
                                            <>
                                                <p className="mb-2 text-muted">Chọn ảnh từ máy tính</p>
                                                <Button
                                                    variant="primary"
                                                    onClick={() => photoInputRef.current?.click()}
                                                >
                                                    Tải lên
                                                </Button>
                                                <input
                                                    {...register("photo")}
                                                    className="d-none"
                                                    type="file"
                                                    accept="image/*"
                                                    onChange={(e) => handleFileChange(e, "photo")}
                                                    ref={photoInputRef}
                                                />
                                            </>
                                        )}
                                    </div>
                                    {errors.photo && (
                                        <Form.Text className="text-danger">
                                            {errors.photo.message?.toString()}
                                        </Form.Text>
                                    )}
                                </Form.Group>
                            </Col>

                            {/* CCCD Mặt Trước */}
                            <Col md={4}>
                                <Form.Group controlId="frontIdCard">
                                    <Form.Label>
                                        CCCD Mặt Trước <span className="text-danger">*</span>
                                    </Form.Label>
                                    <div
                                        className={`d-flex flex-column align-items-center justify-content-center p-3 border rounded ${
                                            errors.frontIdCard ? "border-danger" : "border-secondary"
                                        }`}
                                        style={{ height: "200px", position: "relative" }}
                                    >
                                        {frontIdCardPreview ? (
                                            <>
                                                <Image
                                                    src={frontIdCardPreview}
                                                    thumbnail
                                                    style={{ maxHeight: "150px" }}
                                                />
                                                <Button
                                                    variant="danger"
                                                    size="sm"
                                                    className="position-absolute top-0 end-0"
                                                    onClick={() => removeSelectedFile("frontIdCard")}
                                                >
                                                    &times;
                                                </Button>
                                            </>
                                        ) : (
                                            <>
                                                <p className="mb-2 text-muted">Chọn ảnh từ máy tính</p>
                                                <Button
                                                    variant="primary"
                                                    onClick={() => frontIdCardInputRef.current?.click()}
                                                >
                                                    Tải lên
                                                </Button>
                                                <input
                                                    {...register("frontIdCard")}
                                                    className="d-none"
                                                    type="file"
                                                    accept="image/*"
                                                    onChange={(e) => handleFileChange(e, "frontIdCard")}
                                                    ref={frontIdCardInputRef}
                                                />
                                            </>
                                        )}
                                    </div>
                                    {errors.frontIdCard && (
                                        <Form.Text className="text-danger">
                                            {errors.frontIdCard.message?.toString()}
                                        </Form.Text>
                                    )}
                                </Form.Group>
                            </Col>

                            {/* CCCD Mặt Sau */}
                            <Col md={4}>
                                <Form.Group controlId="backIdCard">
                                    <Form.Label>
                                        CCCD Mặt Sau <span className="text-danger">*</span>
                                    </Form.Label>
                                    <div
                                        className={`d-flex flex-column align-items-center justify-content-center p-3 border rounded ${
                                            errors.backIdCard ? "border-danger" : "border-secondary"
                                        }`}
                                        style={{ height: "200px", position: "relative" }}
                                    >
                                        {backIdCardPreview ? (
                                            <>
                                                <Image
                                                    src={backIdCardPreview}
                                                    thumbnail
                                                    style={{ maxHeight: "150px" }}
                                                />
                                                <Button
                                                    variant="danger"
                                                    size="sm"
                                                    className="position-absolute top-0 end-0"
                                                    onClick={() => removeSelectedFile("backIdCard")}
                                                >
                                                    &times;
                                                </Button>
                                            </>
                                        ) : (
                                            <>
                                                <p className="mb-2 text-muted">Chọn ảnh từ máy tính</p>
                                                <Button
                                                    variant="primary"
                                                    onClick={() => backIdCardInputRef.current?.click()}
                                                >
                                                    Tải lên
                                                </Button>
                                                <input
                                                    {...register("backIdCard")}
                                                    className="d-none"
                                                    type="file"
                                                    accept="image/*"
                                                    onChange={(e) => handleFileChange(e, "backIdCard")}
                                                    ref={backIdCardInputRef}
                                                />
                                            </>
                                        )}
                                    </div>
                                    {errors.backIdCard && (
                                        <Form.Text className="text-danger">
                                            {errors.backIdCard.message?.toString()}
                                        </Form.Text>
                                    )}
                                </Form.Group>
                            </Col>
                        </Row>
                    </Card.Body>
                </Card>

                {/* reCAPTCHA */}
                <Form.Group className="mb-4 text-center">
                    <ReCAPTCHA
                        sitekey="6Le5q2kqAAAAAPNSdBXL3sX_zQHcjy9xFrXsk-mL" // Thay thế bằng Site Key từ Google reCAPTCHA
                        onChange={handleCaptcha}
                    />
                </Form.Group>

                {/* Nút hành động */}
                <div className="d-flex justify-content-end">
                    <Button variant="secondary" className="me-3" onClick={() => navigate(-1)}>
                        Quay lại
                    </Button>
                    <Button variant="primary" type="submit">
                        Đăng ký
                    </Button>
                </div>
            </Form>

            {/* Modal để cắt ảnh */}
            <Modal show={showCropModal} onHide={() => setShowCropModal(false)} size="lg" centered>
                <Modal.Header closeButton>
                    <Modal.Title>Cắt Ảnh</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    {croppingImage && (
                        <>
                            <div
                                className="crop-container"
                                style={{ position: "relative", width: "100%", height: 400 }}
                            >
                                <Cropper
                                    image={croppingImage.imageSrc}
                                    crop={crop}
                                    zoom={zoom}
                                    aspect={cropAspect}
                                    onCropChange={setCrop}
                                    onZoomChange={setZoom}
                                    onCropComplete={onCropComplete}
                                />
                            </div>
                            <div className="d-flex justify-content-center mt-3">
                                <div style={{ width: 300 }}>
                                    <Form.Label htmlFor="zoomRange">Zoom</Form.Label>
                                    <Form.Range
                                        id="zoomRange"
                                        min={1}
                                        max={3}
                                        step={0.1}
                                        value={zoom}
                                        onChange={(e) => setZoom(Number(e.target.value))}
                                    />
                                </div>
                            </div>
                        </>
                    )}
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={() => setShowCropModal(false)}>
                        Hủy
                    </Button>
                    <Button variant="primary" onClick={handleCropSave}>
                        Lưu
                    </Button>
                </Modal.Footer>
            </Modal>
        </div>
    )
}

export default StudentRegistration

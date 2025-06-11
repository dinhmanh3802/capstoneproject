// StudentInfo.tsx
import React, { useState, useEffect, useCallback, useRef } from "react"
import { SD_ProcessStatus, SD_Role_Name } from "../../../utility/SD"
import { format } from "date-fns"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { error } from "../../../utility/Message"
import { toast } from "react-toastify"
import Cropper from "react-easy-crop"
import Modal from "react-bootstrap/Modal"
import Button from "react-bootstrap/Button"
import imageCompression from "browser-image-compression"
import { Image, Row, Col, Form, Card } from "react-bootstrap"
import { useUpdateStudentMutation } from "../../../apis/studentApi"
import { toastNotify } from "../../../helper"
import { apiResponse } from "../../../interfaces"
import ConfirmationPopup from "../../commonCp/ConfirmationPopup"
import { useSelector } from "react-redux"
import { RootState } from "../../../store/store"

// Constants
const MAX_FILE_SIZE = 5000000 // 5MB
const ACCEPTED_IMAGE_TYPES = ["image/jpeg", "image/jpg", "image/png"]

// Form schema
const formSchema = z.object({
    fullName: z
        .string()
        .trim()
        .min(1, { message: `${error.fullNameRequired}` })
        .max(100, { message: `${error.fullNameTooLong}` })
        .regex(/^[A-Za-zÀ-Ỹà-ỹ\s]+$/, {
            message: `${error.invalidFullName}`,
        })
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
    nationalId: z
        .string()
        .trim()
        .regex(/^\d{9,12}$/, { message: `${error.idNumberLengthError}` }),
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
        .regex(/^[A-Za-zÀ-Ỹà-ỹ\s]+$/, {
            message: `${error.invalidParentName}`,
        })
        .refine((value) => !/\s{2,}/.test(value), { message: `${error.noConsecutiveSpacesParentName}` })
        .refine((value) => value.trim() === value, { message: `${error.noLeadingOrTrailingSpacesParentName}` }),

    emergencyContact: z
        .string()
        .trim()
        .regex(/^(0[3|5|7|8|9])+([0-9]{8})$/, { message: `${error.invalidPhone}` })
        .min(10, { message: `${error.phoneRequired}` })
        .max(10, { message: `${error.phoneTooLong}` }),

    image: z
        .any()
        .optional()
        .nullable()
        .refine(
            (files) => {
                if (!files || files.length === 0) return true // Image is optional
                return files.length === 1
            },
            { message: `${error.photoRequired}` },
        )
        .refine(
            (files) => {
                if (!files || files.length === 0) return true
                return files[0].size <= MAX_FILE_SIZE
            },
            { message: `${error.photoMaxSize}` },
        )
        .refine(
            (files) => {
                if (!files || files.length === 0) return true
                return ACCEPTED_IMAGE_TYPES.includes(files[0].type)
            },
            { message: `${error.photoInvalidType}` },
        ),

    nationalImageFront: z
        .any()
        .optional()
        .nullable()
        .refine(
            (files) => {
                if (!files || files.length === 0) return true
                return files.length === 1
            },
            { message: `${error.frontIdCardRequired}` },
        )
        .refine(
            (files) => {
                if (!files || files.length === 0) return true
                return files[0].size <= MAX_FILE_SIZE
            },
            { message: `${error.frontIdCardMaxSize}` },
        )
        .refine(
            (files) => {
                if (!files || files.length === 0) return true
                return ACCEPTED_IMAGE_TYPES.includes(files[0].type)
            },
            { message: `${error.frontIdCardInvalidType}` },
        ),

    nationalImageBack: z
        .any()
        .optional()
        .nullable()
        .refine(
            (files) => {
                if (!files || files.length === 0) return true
                return files.length === 1
            },
            { message: `${error.backIdCardRequired}` },
        )
        .refine(
            (files) => {
                if (!files || files.length === 0) return true
                return files[0].size <= MAX_FILE_SIZE
            },
            { message: `${error.backIdCardMaxSize}` },
        )
        .refine(
            (files) => {
                if (!files || files.length === 0) return true
                return ACCEPTED_IMAGE_TYPES.includes(files[0].type)
            },
            { message: `${error.backIdCardInvalidType}` },
        ),
    academicPerformance: z.string().min(1, { message: `${error.academicPerformanceRequired}` }),
    conduct: z.string().min(1, { message: `${error.conductRequired}` }),
})

type FormData = z.infer<typeof formSchema>

function StudentInfo({ studentApplication, refetchStudent }: any) {
    const [isEditing, setIsEditing] = useState(false)
    const [updateStudent, { isLoading }] = useUpdateStudentMutation()
    const currentUserRole = useSelector((state: RootState) => state.auth.user?.role)

    const {
        register,
        handleSubmit,
        setValue,
        trigger,
        formState: { errors },
        reset,
    } = useForm<FormData>({
        resolver: zodResolver(formSchema),
    })

    const [loading, setLoading] = useState(false)

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
    const [cropAspect, setCropAspect] = useState<number>(1 / 1) // Default aspect ratio for photo

    // Refs to reset input fields
    const photoInputRef = useRef<HTMLInputElement | null>(null)
    const frontIdCardInputRef = useRef<HTMLInputElement | null>(null)
    const backIdCardInputRef = useRef<HTMLInputElement | null>(null)

    // States to store file previews
    const [photoPreview, setPhotoPreview] = useState<string | null>(null)
    const [frontIdCardPreview, setFrontIdCardPreview] = useState<string | null>(null)
    const [backIdCardPreview, setBackIdCardPreview] = useState<string | null>(null)

    // Initialize form data
    useEffect(() => {
        if (studentApplication.student) {
            const initialData = {
                ...studentApplication.student,
                dateOfBirth: studentApplication.student.dateOfBirth
                    ? format(new Date(studentApplication.student.dateOfBirth), "yyyy-MM-dd")
                    : "",
                gender: studentApplication.student.gender?.toString() || "0",
                image: null,
                nationalImageFront: null,
                nationalImageBack: null,
            }
            reset(initialData)

            // Set previews
            setPhotoPreview(studentApplication.student.image || null)
            setFrontIdCardPreview(studentApplication.student.nationalImageFront || null)
            setBackIdCardPreview(studentApplication.student.nationalImageBack || null)
        }
    }, [studentApplication, reset])

    const handleEditClick = () => {
        setIsEditing(true)
    }

    const handleCancelClick = () => {
        setIsEditing(false)
        // Reset form data to original
        if (studentApplication.student) {
            const initialData = {
                ...studentApplication.student,
                dateOfBirth: studentApplication.student.dateOfBirth
                    ? format(new Date(studentApplication.student.dateOfBirth), "yyyy-MM-dd")
                    : "",
                gender: studentApplication.student.gender?.toString() || "0",
                image: null,
                nationalImageFront: null,
                nationalImageBack: null,
            }
            reset(initialData)

            // Reset previews
            setPhotoPreview(studentApplication.student.image || null)
            setFrontIdCardPreview(studentApplication.student.nationalImageFront || null)
            setBackIdCardPreview(studentApplication.student.nationalImageBack || null)
        }
    }
    const [isConfirmPopupOpen, setIsConfirmPopupOpen] = useState(false)
    const [formDataToSubmit, setFormDataToSubmit] = useState<FormData | null>(null)
    // Modified onSubmit handler
    const onSubmit = async (data: FormData) => {
        setFormDataToSubmit(data)
        setIsConfirmPopupOpen(true)
    }

    // Function to handle confirmation
    const confirmSave = async () => {
        if (formDataToSubmit) {
            setIsConfirmPopupOpen(false)
            setLoading(true)
            try {
                const studentId = studentApplication.student?.id // Adjust the field name if necessary
                const response: apiResponse = await updateStudent({
                    studentId: studentId,
                    studentData: formDataToSubmit,
                })
                if (response.data?.isSuccess) {
                    toastNotify("Cập nhật thông tin sinh viên thành công", "success")
                    setIsEditing(false)
                    refetchStudent()
                } else {
                    const errorMessage = response.data?.errorMessages?.join(", ") || "Có lỗi xảy ra"
                    toastNotify(errorMessage, "error")
                }
            } catch (error) {
                toastNotify("Cập nhật thông tin sinh viên thất bại", "error")
            } finally {
                setLoading(false)
            }
        }
    }

    // Function to handle cancellation
    const cancelSave = () => {
        setFormDataToSubmit(null)
        setIsConfirmPopupOpen(false)
    }

    // Cropping functions
    const onCropComplete = useCallback((croppedArea: any, croppedAreaPixels: any) => {
        setCroppedAreaPixels(croppedAreaPixels)
    }, [])

    const createImage = (url: string): Promise<HTMLImageElement> =>
        new Promise((resolve, reject) => {
            const image = document.createElement("img")
            image.addEventListener("load", () => resolve(image))
            image.addEventListener("error", (error) => reject(error))
            image.setAttribute("crossOrigin", "anonymous")
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
            const fileURL = URL.createObjectURL(file)

            // Set aspect ratio based on field
            if (field === "image") {
                setCropAspect(1 / 1) // 4x6
            } else if (field === "nationalImageFront" || field === "nationalImageBack") {
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

                // Set value for the corresponding field as FileList
                const dataTransfer = new DataTransfer()
                dataTransfer.items.add(compressedFile)
                setValue(croppingImage.field, dataTransfer.files, { shouldValidate: true })
                await trigger(croppingImage.field) // Trigger validation for the specific field

                // Update preview and reset states
                const compressedFileURL = URL.createObjectURL(compressedFile)
                if (croppingImage.field === "image") {
                    setPhotoPreview(compressedFileURL)
                } else if (croppingImage.field === "nationalImageFront") {
                    setFrontIdCardPreview(compressedFileURL)
                } else if (croppingImage.field === "nationalImageBack") {
                    setBackIdCardPreview(compressedFileURL)
                }

                // Reset input field via ref
                if (croppingImage.field === "image" && photoInputRef.current) {
                    photoInputRef.current.value = ""
                } else if (croppingImage.field === "nationalImageFront" && frontIdCardInputRef.current) {
                    frontIdCardInputRef.current.value = ""
                } else if (croppingImage.field === "nationalImageBack" && backIdCardInputRef.current) {
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
        if (field === "image") {
            setPhotoPreview(null)
            setValue("image", null, { shouldValidate: true })
        } else if (field === "nationalImageFront") {
            setFrontIdCardPreview(null)
            setValue("nationalImageFront", null, { shouldValidate: true })
        } else if (field === "nationalImageBack") {
            setBackIdCardPreview(null)
            setValue("nationalImageBack", null, { shouldValidate: true })
        }
    }

    return (
        <>
            <Form onSubmit={handleSubmit(onSubmit)} className="row g-3 mt-2">
                <Row>
                    <Col md={9}>
                        <Row className="g-3">
                            {/* Họ và tên */}
                            <Col md={4}>
                                <Form.Group controlId="fullName">
                                    <Form.Label>Họ và tên</Form.Label>
                                    <Form.Control
                                        type="text"
                                        {...register("fullName")}
                                        disabled={!isEditing}
                                        isInvalid={!!errors.fullName}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.fullName?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Giới tính */}
                            <Col md={4}>
                                <Form.Group controlId="gender">
                                    <Form.Label>Giới tính</Form.Label>
                                    {isEditing ? (
                                        <Form.Select
                                            {...register("gender")}
                                            isInvalid={!!errors.gender}
                                            disabled={!isEditing}
                                        >
                                            <option value="">Chọn giới tính</option>
                                            <option value="0">Nam</option>
                                            <option value="1">Nữ</option>
                                        </Form.Select>
                                    ) : (
                                        <Form.Control
                                            type="text"
                                            value={studentApplication?.student?.gender === 0 ? "Nam" : "Nữ"}
                                            disabled
                                        />
                                    )}
                                    {errors.gender && (
                                        <Form.Control.Feedback type="invalid">
                                            {errors.gender?.message}
                                        </Form.Control.Feedback>
                                    )}
                                </Form.Group>
                            </Col>

                            {/* Ngày sinh */}
                            <Col md={4}>
                                <Form.Group controlId="dateOfBirth">
                                    <Form.Label>Ngày sinh</Form.Label>
                                    <Form.Control
                                        type={isEditing ? "date" : "text"}
                                        {...register("dateOfBirth")}
                                        disabled={!isEditing}
                                        isInvalid={!!errors.dateOfBirth}
                                        value={
                                            isEditing
                                                ? undefined
                                                : studentApplication.student?.dateOfBirth
                                                ? format(new Date(studentApplication.student.dateOfBirth), "dd-MM-yyyy")
                                                : ""
                                        }
                                    />

                                    <Form.Control.Feedback type="invalid">
                                        {errors.dateOfBirth?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Địa chỉ */}
                            <Col md={4}>
                                <Form.Group controlId="address">
                                    <Form.Label>Địa chỉ</Form.Label>
                                    <Form.Control
                                        type="text"
                                        {...register("address")}
                                        disabled={!isEditing}
                                        isInvalid={!!errors.address}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.address?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Học lực */}
                            <Col md={2}>
                                <Form.Group controlId="academicPerformance">
                                    <Form.Label>Học lực</Form.Label>
                                    <Form.Control
                                        type="text"
                                        {...register("academicPerformance")}
                                        disabled={!isEditing}
                                        isInvalid={!!errors.academicPerformance}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.academicPerformance?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Hạnh kiểm */}
                            <Col md={2}>
                                <Form.Group controlId="conduct">
                                    <Form.Label>Hạnh kiểm</Form.Label>
                                    <Form.Control
                                        type="text"
                                        {...register("conduct")}
                                        disabled={!isEditing}
                                        isInvalid={!!errors.conduct}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.conduct?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>
                            {/* Số CMND */}
                            <Col md={4}>
                                <Form.Group controlId="nationalId">
                                    <Form.Label>Số CMND</Form.Label>
                                    <Form.Control
                                        type="text"
                                        {...register("nationalId")}
                                        disabled={!isEditing}
                                        isInvalid={!!errors.nationalId}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.nationalId?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Họ và tên cha/mẹ */}
                            <Col md={4}>
                                <Form.Group controlId="parentName">
                                    <Form.Label>Phụ huynh</Form.Label>
                                    <Form.Control
                                        type="text"
                                        {...register("parentName")}
                                        disabled={!isEditing}
                                        isInvalid={!!errors.parentName}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.parentName?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Điện thoại phụ huynh */}
                            <Col md={4}>
                                <Form.Group controlId="emergencyContact">
                                    <Form.Label>Điện thoại phụ huynh</Form.Label>
                                    <Form.Control
                                        type="text"
                                        {...register("emergencyContact")}
                                        disabled={!isEditing}
                                        isInvalid={!!errors.emergencyContact}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.emergencyContact?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>

                            {/* Email */}
                            <Col md={4}>
                                <Form.Group controlId="email">
                                    <Form.Label>Email</Form.Label>
                                    <Form.Control
                                        type="email"
                                        {...register("email")}
                                        disabled={!isEditing}
                                        isInvalid={!!errors.email}
                                    />
                                    <Form.Control.Feedback type="invalid">
                                        {errors.email?.message}
                                    </Form.Control.Feedback>
                                </Form.Group>
                            </Col>
                        </Row>
                    </Col>
                    <Col md={3}>
                        {!isEditing && (
                            <div>
                                <label htmlFor="endDate" className="form-label">
                                    <span style={{ visibility: "hidden" }}>.</span>
                                </label>
                                <div className="text-center">
                                    <img
                                        src={`${studentApplication.student?.image}`}
                                        alt="Front ID"
                                        className="img-fluid"
                                        style={{ maxHeight: "14rem" }}
                                    />
                                </div>
                            </div>
                        )}
                        {isEditing && (
                            <Form.Group controlId="image">
                                <Form.Label>Ảnh thẻ (4x6)</Form.Label>
                                <div
                                    className={`d-flex flex-column align-items-center justify-content-center p-3 border rounded ${
                                        errors.image ? "border-danger" : "border-secondary"
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
                                                onClick={() => removeSelectedFile("image")}
                                            >
                                                &times;
                                            </Button>
                                        </>
                                    ) : (
                                        <>
                                            <p className="mb-2 text-muted">Chọn ảnh từ máy tính</p>
                                            <Button variant="primary" onClick={() => photoInputRef.current?.click()}>
                                                Tải lên
                                            </Button>
                                            <input
                                                {...register("image")}
                                                className="d-none"
                                                type="file"
                                                accept="image/*"
                                                onChange={(e) => handleFileChange(e, "image")}
                                                ref={photoInputRef}
                                            />
                                        </>
                                    )}
                                </div>
                                {errors.image && (
                                    <Form.Text className="text-danger">{errors.image.message?.toString()}</Form.Text>
                                )}
                            </Form.Group>
                        )}
                    </Col>
                </Row>

                {/* Image Uploads */}
                {isEditing && (
                    <Row className="g-4">
                        {/* CCCD Mặt Trước */}
                        <Col md={6}>
                            <Form.Group controlId="nationalImageFront">
                                <Form.Label>CCCD Mặt Trước</Form.Label>
                                <div
                                    className={`d-flex flex-column align-items-center justify-content-center p-3 border rounded ${
                                        errors.nationalImageFront ? "border-danger" : "border-secondary"
                                    }`}
                                    style={{ height: "200px", position: "relative" }}
                                >
                                    {frontIdCardPreview ? (
                                        <>
                                            <Image src={frontIdCardPreview} thumbnail style={{ maxHeight: "150px" }} />
                                            <Button
                                                variant="danger"
                                                size="sm"
                                                className="position-absolute top-0 end-0"
                                                onClick={() => removeSelectedFile("nationalImageFront")}
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
                                                {...register("nationalImageFront")}
                                                className="d-none"
                                                type="file"
                                                accept="image/*"
                                                onChange={(e) => handleFileChange(e, "nationalImageFront")}
                                                ref={frontIdCardInputRef}
                                            />
                                        </>
                                    )}
                                </div>
                                {errors.nationalImageFront && (
                                    <Form.Text className="text-danger">
                                        {errors.nationalImageFront.message?.toString()}
                                    </Form.Text>
                                )}
                            </Form.Group>
                        </Col>

                        {/* CCCD Mặt Sau */}
                        <Col md={6}>
                            <Form.Group controlId="nationalImageBack">
                                <Form.Label>CCCD Mặt Sau</Form.Label>
                                <div
                                    className={`d-flex flex-column align-items-center justify-content-center p-3 border rounded ${
                                        errors.nationalImageBack ? "border-danger" : "border-secondary"
                                    }`}
                                    style={{ height: "200px", position: "relative" }}
                                >
                                    {backIdCardPreview ? (
                                        <>
                                            <Image src={backIdCardPreview} thumbnail style={{ maxHeight: "150px" }} />
                                            <Button
                                                variant="danger"
                                                size="sm"
                                                className="position-absolute top-0 end-0"
                                                onClick={() => removeSelectedFile("nationalImageBack")}
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
                                                {...register("nationalImageBack")}
                                                className="d-none"
                                                type="file"
                                                accept="image/*"
                                                onChange={(e) => handleFileChange(e, "nationalImageBack")}
                                                ref={backIdCardInputRef}
                                            />
                                        </>
                                    )}
                                </div>
                                {errors.nationalImageBack && (
                                    <Form.Text className="text-danger">
                                        {errors.nationalImageBack.message?.toString()}
                                    </Form.Text>
                                )}
                            </Form.Group>
                        </Col>
                    </Row>
                )}

                {/* Display Images when not editing */}
                {!isEditing && (
                    <>
                        <div className="col-md-6">
                            <label className="form-label fw-medium">Ảnh CCCD mặt trước</label>
                            <div className="border p-3 text-center">
                                <img
                                    src={`${studentApplication.student?.nationalImageFront}`}
                                    alt="Front ID"
                                    className="img-fluid"
                                    style={{ maxHeight: "20rem" }}
                                />
                            </div>
                        </div>
                        <div className="col-md-6">
                            <label className="form-label fw-medium">Ảnh CCCD mặt sau</label>
                            <div className="border p-3 text-center">
                                <img
                                    src={`${studentApplication.student?.nationalImageBack}`}
                                    alt="Back ID"
                                    className="img-fluid"
                                    style={{ maxHeight: "20rem" }}
                                />
                            </div>
                        </div>
                    </>
                )}

                {/* Modal for cropping */}
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
                {(currentUserRole == SD_Role_Name.SECRETARY || currentUserRole == SD_Role_Name.MANAGER) && (
                    <div className="mb-3 text-edit text-end">
                        {!isEditing ? (
                            <button type="button" className="btn btn-primary" onClick={handleEditClick}>
                                Sửa
                            </button>
                        ) : (
                            <>
                                <Button variant="success" type="submit" disabled={loading || isLoading}>
                                    Lưu
                                </Button>
                                <button type="button" className="btn btn-secondary ms-2" onClick={handleCancelClick}>
                                    Hủy
                                </button>
                            </>
                        )}
                    </div>
                )}
            </Form>

            <ConfirmationPopup
                isOpen={isConfirmPopupOpen}
                onClose={cancelSave}
                onConfirm={confirmSave}
                message="Xác nhận lưu thông tin sinh viên?"
            />
        </>
    )
}

export default StudentInfo

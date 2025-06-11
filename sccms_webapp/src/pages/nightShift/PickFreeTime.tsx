import React, { useEffect, useState } from "react"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"
import { FreeDaysPicker, MainLoader } from "../../components/Page"
import { useGetCourseByIdQuery, useGetCourseQuery } from "../../apis/courseApi"
import { toZonedTime } from "date-fns-tz"
import { formatInTimeZone } from "date-fns-tz"
import Select, { SingleValue } from "react-select"
import { useCreateStaffFreeTimeMutation, useGetAllStaffFreeTimesQuery } from "../../apis/staffFreeTimeApi"
import { apiResponse } from "../../interfaces"
import { toastNotify } from "../../helper"

const timeZone = "Asia/Ho_Chi_Minh"

interface staffFreeTimeDto {
    userId: number
    courseId: number
    freeDates: string[]
}

const isValidDate = (date: any): date is Date => {
    return date instanceof Date && !isNaN(date.getTime())
}

function PickFreeTime() {
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)
    const currentUserId = useSelector((state: RootState) => state.auth.user?.userId)
    const [createStaffFreeTime] = useCreateStaffFreeTimeMutation()
    const [selectedCourseId, setSelectedCourseId] = useState<number | undefined>(undefined)
    useEffect(() => {
        if (currentCourse?.id) {
            setSelectedCourseId(currentCourse.id)
        }
    }, [currentCourse])
    // const [selectedCourseId, setSelectedCourseId] = useState(currentCourse?.id)
    // useEffect(() => {
    //     setSelectedCourseId(currentCourse?.id)
    // }, [currentCourse])

    // Fetch course details by selectedCourseId
    const { data: courseData, isLoading: courseLoading } = useGetCourseByIdQuery(selectedCourseId || 0, {
        skip: !selectedCourseId,
    })

    // Fetch user's free times
    const {
        data: freeTime,
        isLoading: freeTimeLoading,
        refetch: refetchFreeTime,
    } = useGetAllStaffFreeTimesQuery({ userId: currentUserId })

    const [courseList, setCourseList] = useState<any[]>([])
    const { data: coursesStatus1 } = useGetCourseQuery({ status: 1 })
    const { data: coursesStatus2 } = useGetCourseQuery({ status: 2 })

    useEffect(() => {
        const mergedCourses = []
        if (coursesStatus1?.result) mergedCourses.push(...coursesStatus1?.result)
        if (coursesStatus2?.result) mergedCourses.push(...coursesStatus2?.result)

        setCourseList(mergedCourses)
    }, [coursesStatus1, coursesStatus2])

    const courseOptions: any = courseList?.map((course) => ({
        value: course.id,
        label: course.courseName,
    }))

    const preselectedDates =
        !freeTimeLoading && freeTime?.result
            ? freeTime.result?.map((item) => toZonedTime(new Date(item.date), timeZone))
            : []
    const startDate = courseData?.result.startDate
        ? toZonedTime(new Date(courseData?.result.startDate), timeZone)
        : null

    const endDate = courseData?.result.endDate ? toZonedTime(new Date(courseData?.result.endDate), timeZone) : null

    const handleCourseChange = (selectedOption: SingleValue<any>) => {
        setSelectedCourseId(selectedOption?.value)
    }

    const handleSubmit = async (selectedDates: string[]) => {
        const newRoom: staffFreeTimeDto = {
            userId: currentUserId || 0,
            courseId: selectedCourseId || 0,
            freeDates: selectedDates?.map((date) => date),
        }
        try {
            const response: apiResponse = await createStaffFreeTime(newRoom).unwrap()
            toastNotify("Cập nhật thành công!", "success")
            refetchFreeTime()
        } catch (error: any) {
            const errorMessages = error?.data?.errorMessages?.join(", ") || "Có lỗi xảy ra!"
            toastNotify(errorMessages, "error")
        }
    }
    const getToday = () => {
        const now = new Date()
        const vietnamNow = toZonedTime(now, timeZone)
        vietnamNow.setHours(0, 0, 0, 0)
        return vietnamNow
    }
    if (courseLoading || freeTimeLoading) return <MainLoader />

    const formattedEndDate =
        courseData?.result.freeTimeApplicationEndDate &&
        isValidDate(new Date(courseData.result.freeTimeApplicationEndDate))
            ? formatInTimeZone(new Date(courseData.result.freeTimeApplicationEndDate), timeZone, "dd/MM/yyyy")
            : null

    const formattedStartDate =
        courseData?.result.freeTimeApplicationStartDate &&
        isValidDate(new Date(courseData.result.freeTimeApplicationStartDate))
            ? formatInTimeZone(new Date(courseData.result.freeTimeApplicationStartDate), timeZone, "dd/MM/yyyy")
            : null

    const formattedToday = formatInTimeZone(getToday(), timeZone, "dd/MM/yyyy")

    return (
        <div className="container">
            <div className="mt-0 mb-3">
                <h3 className="fw-bold primary-color">Đăng ký ngày trực</h3>
            </div>
            {/* Dropdown chọn khóa tu */}
            <div className="col-4">
                <div className="mb-3">
                    <Select
                        options={courseOptions}
                        value={courseOptions?.find((option) => option.value === selectedCourseId)}
                        onChange={handleCourseChange}
                        isClearable={false}
                        className="z-3"
                    />
                </div>
            </div>
            {formattedEndDate && formattedStartDate ? (
                formattedEndDate < formattedToday ? (
                    <div className="alert alert-warning" role="alert">
                        Đã hết thời gian đăng ký trực cho khóa tu này
                    </div>
                ) : (
                    <div className="row">
                        <div className="col-md-6">
                            {startDate && endDate && (
                                <FreeDaysPicker
                                    startDate={startDate}
                                    endDate={endDate}
                                    preselectedDates={preselectedDates}
                                    onSubmit={handleSubmit}
                                    timeZone={timeZone}
                                />
                            )}
                        </div>
                        <div className="col-md-6">
                            <div className="alert alert-warning" role="alert">
                                Thời gian đăng ký từ ngày {formattedStartDate} - {formattedEndDate}
                            </div>
                        </div>
                    </div>
                )
            ) : (
                <div className="alert alert-warning" role="alert">
                    Đã hết thời gian đăng ký trực cho khóa tu này
                </div>
            )}
        </div>
    )
}

export default PickFreeTime

// CourseDashboard.tsx

import React, { useEffect, useState, useMemo } from "react"
import { useLocation, useNavigate } from "react-router-dom"
import {
    Card,
    Grid,
    Typography,
    CircularProgress,
    Box,
    Divider,
    useTheme,
    useMediaQuery,
    Paper,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
} from "@mui/material"
import { People as PeopleIcon, VolunteerActivism as VolunteerIcon, Feedback as FeedbackIcon } from "@mui/icons-material"
import { Doughnut, Bar } from "react-chartjs-2"
import {
    Chart as ChartJS,
    Title,
    Tooltip as ChartTooltip,
    Legend,
    ArcElement,
    CategoryScale,
    LinearScale,
    BarElement,
} from "chart.js"
import {
    useGetCourseDashboardQuery,
    useGetStudentRegistrationsPerCourseQuery,
    useGetVolunteerRegistrationsPerCourseQuery,
} from "../../apis/courseApi"
import { useSelector } from "react-redux"
import { RootState } from "../../store/store"

// Register Chart.js components
ChartJS.register(Title, ChartTooltip, Legend, ArcElement, CategoryScale, LinearScale, BarElement)

// Define TypeScript interfaces
interface RegistrationPerCourseDto {
    courseId: number
    courseName: string
    registrationCount: number
}

interface DashboardDto {
    totalRegisteredStudents?: number
    totalStudents?: number
    totalMaleStudents?: number
    totalRegisteredVolunteers?: number
    totalVolunteers?: number
    totalMaleVolunteers?: number
    attendanceRate?: number
    graduationRate?: number
    totalFeedbacks?: number
}

// Reusable Summary Card Component with Icon and Clickable Feature
const SummaryCard = ({
    title,
    value,
    color,
    IconComponent,
    onClick,
}: {
    title: string
    value: number
    color: string
    IconComponent: React.ElementType
    onClick: () => void
}) => (
    <Card
        onClick={onClick}
        sx={{
            padding: 3,
            display: "flex",
            alignItems: "center",
            backgroundColor: "#fff",
            boxShadow: "0 4px 12px rgba(0, 0, 0, 0.1)",
            borderRadius: "16px",
            cursor: "pointer",
            transition: "transform 0.3s ease, box-shadow 0.3s ease",
            "&:hover": {
                transform: "translateY(-5px)",
                boxShadow: "0 8px 20px rgba(0, 0, 0, 0.15)",
            },
        }}
    >
        <Box
            sx={{
                marginRight: 2,
                padding: 2,
                backgroundColor: color,
                borderRadius: "50%",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
            }}
        >
            <IconComponent sx={{ color: "#fff", fontSize: 30 }} />
        </Box>
        <Box>
            <Typography variant="subtitle1" sx={{ color: color, fontWeight: 500 }}>
                {title}
            </Typography>
            <Typography variant="h5" sx={{ color: "#333", fontWeight: 700 }}>
                {value}
            </Typography>
        </Box>
    </Card>
)

// Reusable Doughnut Chart Component with Enhanced Styling
const DoughnutChart = ({
    title,
    data,
    labels,
    backgroundColors,
    showPercentage = false, // Thêm prop showPercentage
}: {
    title: string
    data: number[]
    labels: string[]
    backgroundColors: string[]
    showPercentage?: boolean // Prop tùy chọn
}) => {
    const theme = useTheme()
    const isDarkMode = theme.palette.mode === "dark"

    const chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: "bottom" as const,
                labels: {
                    color: isDarkMode ? "#fff" : "#333",
                    font: {
                        size: 14,
                    },
                },
            },
            tooltip: {
                enabled: true,
                backgroundColor: isDarkMode ? "#424242" : "#fff",
                titleColor: isDarkMode ? "#fff" : "#333",
                bodyColor: isDarkMode ? "#ddd" : "#555",
                borderColor: isDarkMode ? "#555" : "#ccc",
                borderWidth: 1,
                callbacks: {
                    label: function (context: any) {
                        return `${context.label}: ${context.raw}${showPercentage ? "%" : ""}`
                    },
                },
            },
            datalabels: showPercentage
                ? {
                      color: theme.palette.text.primary,
                      formatter: (value: number, context: any) => {
                          const total = context.chart.data.datasets[0].data.reduce((a: number, b: number) => a + b, 0)
                          const percentage = total > 0 ? (value / total) * 100 : 0
                          return `${Math.round(percentage)}%`
                      },
                      font: {
                          weight: "bold" as const,
                          size: 14,
                      },
                  }
                : false, // Vô hiệu hóa datalabels nếu không cần
        },
    }

    const chartData = {
        labels,
        datasets: [
            {
                data,
                backgroundColor: backgroundColors,
                borderColor: theme.palette.background.paper,
                borderWidth: 2,
                hoverOffset: 10,
            },
        ],
    }

    return (
        <Paper
            elevation={3}
            sx={{
                padding: 3,
                backgroundColor: theme.palette.background.paper,
                borderRadius: "16px",
                height: "100%",
                display: "flex",
                flexDirection: "column",
                transition: "transform 0.3s ease, box-shadow 0.3s ease",
                "&:hover": {
                    transform: "translateY(-5px)",
                    boxShadow: "0 8px 20px rgba(0, 0, 0, 0.15)",
                },
            }}
        >
            <Typography variant="h6" sx={{ color: theme.palette.primary.main, mb: 2, textAlign: "center" }}>
                {title}
            </Typography>
            <Box sx={{ position: "relative", flexGrow: 1 }}>
                <Doughnut data={chartData} options={chartOptions} />
            </Box>
        </Paper>
    )
}

const CourseDashboard = () => {
    const location = useLocation()
    const navigate = useNavigate()
    const theme = useTheme()
    const isSmallScreen = useMediaQuery(theme.breakpoints.down("sm"))

    const searchParams = useMemo(() => new URLSearchParams(location.search), [location.search])
    const courseIdParam = searchParams.get("courseId")
    const [selectedCourseId, setSelectedCourseId] = useState<number | undefined>(
        courseIdParam ? Number(courseIdParam) : undefined,
    )

    // Get list of courses from Redux store
    const listCourseFromStore = useSelector((state: RootState) => state.courseStore.courses ?? [])
    const currentCourse = useSelector((state: RootState) => state.courseStore.currentCourse)

    // Sync selectedCourseId with currentCourse from store on mount
    useEffect(() => {
        if (currentCourse?.id) {
            setSelectedCourseId(currentCourse.id)
            navigate(`?courseId=${currentCourse.id}`, { replace: true })
        }
    }, [currentCourse, navigate])

    // Handle course selection change
    const handleCourseChange: any = (event: React.ChangeEvent<{ value: unknown }>) => {
        const newCourseId = event.target.value as number
        setSelectedCourseId(newCourseId)
        navigate(`?courseId=${newCourseId}`)
    }

    // Time ranges for the registration over time charts
    const timeRanges = [
        { label: "1 Năm", value: 1 },
        { label: "3 Năm", value: 3 },
        { label: "5 Năm", value: 5 },
        { label: "10 Năm +", value: 100 },
    ]

    // State for selected time ranges
    const [studentTimeRange, setStudentTimeRange] = useState<number>(3) // Default 3 years
    const [volunteerTimeRange, setVolunteerTimeRange] = useState<number>(3) // Default 3 years

    // Fetch dashboard data based on selectedCourseId
    const {
        data: dashboardData,
        error,
        isLoading,
    } = useGetCourseDashboardQuery(selectedCourseId || 0, {
        skip: !selectedCourseId,
    })

    // Fetch registration per course data
    const {
        data: studentRegistrationsData,
        isLoading: isStudentLoading,
        error: studentError,
    } = useGetStudentRegistrationsPerCourseQuery(studentTimeRange)
    const {
        data: volunteerRegistrationsData,
        isLoading: isVolunteerLoading,
        error: volunteerError,
    } = useGetVolunteerRegistrationsPerCourseQuery(volunteerTimeRange)

    // Access `result` from API response
    const studentRegistrations = studentRegistrationsData?.result || []
    const volunteerRegistrations = volunteerRegistrationsData?.result || []

    // Helper function to extract year from courseName
    const extractYear = (courseName: string): number => {
        const match = courseName.match(/\b\d{4}\b/)
        return match ? parseInt(match[0], 10) : 0
    }

    // Sort student registrations by year ascending
    const sortedStudentRegistrations = useMemo(() => {
        return [...studentRegistrations].sort((a, b) => extractYear(a.courseName) - extractYear(b.courseName))
    }, [studentRegistrations])

    // Sort volunteer registrations by year ascending
    const sortedVolunteerRegistrations = useMemo(() => {
        return [...volunteerRegistrations].sort((a, b) => extractYear(a.courseName) - extractYear(b.courseName))
    }, [volunteerRegistrations])

    const {
        totalStudents = 0,
        totalVolunteers = 0,
        totalFeedbacks = 0,
        totalRegisteredStudents = 1, // Prevent division by zero
        totalRegisteredVolunteers = 1,
        totalMaleStudents = 0,
        totalMaleVolunteers = 0,
        attendanceRate = 0,
        graduationRate = 0,
    } = dashboardData?.result || {}

    const primaryColor = theme.palette.primary.main
    const infoColor = theme.palette.info.main
    const successColor = theme.palette.success.main
    const warningColor = theme.palette.warning.main

    // Navigation handlers
    const handleNavigateStudents = () => {
        navigate(`/students?courseId=${selectedCourseId}`)
    }

    const handleNavigateVolunteers = () => {
        navigate(`/volunteers?courseId=${selectedCourseId}`)
    }

    const handleNavigateFeedbacks = () => {
        navigate(`/feedback?courseId=${selectedCourseId}`)
    }

    // Prepare course options for dropdown
    const courseOptions = useMemo(
        () =>
            listCourseFromStore.map((course) => ({
                id: course.id,
                name: course.courseName,
            })),
        [listCourseFromStore],
    )
    // Làm tròn tỷ lệ phần trăm
    const roundedAttendanceRate = Math.round(attendanceRate)
    const roundedGraduationRate = Math.round(graduationRate)
    // Determine what to display based on loading and error states
    const content = useMemo(() => {
        if (isLoading || isStudentLoading || isVolunteerLoading) {
            return (
                <Box
                    sx={{
                        display: "flex",
                        justifyContent: "center",
                        alignItems: "center",
                        height: "80vh",
                    }}
                >
                    <CircularProgress />
                </Box>
            )
        }

        if (error || studentError || volunteerError) {
            return (
                <Box
                    sx={{
                        display: "flex",
                        justifyContent: "center",
                        alignItems: "center",
                        height: "80vh",
                    }}
                >
                    <Typography color="error" variant="h6">
                        Lỗi khi tải dữ liệu
                    </Typography>
                </Box>
            )
        }

        return (
            <>
                {/* Summary Cards */}
                <Grid container spacing={4} sx={{ mb: 4 }}>
                    <Grid item xs={12} sm={6} md={4}>
                        <SummaryCard
                            title="Tổng số khóa sinh"
                            value={totalStudents}
                            color={primaryColor}
                            IconComponent={PeopleIcon}
                            onClick={handleNavigateStudents}
                        />
                    </Grid>
                    <Grid item xs={12} sm={6} md={4}>
                        <SummaryCard
                            title="Tổng số tình nguyện viên"
                            value={totalVolunteers}
                            color={infoColor}
                            IconComponent={VolunteerIcon}
                            onClick={handleNavigateVolunteers}
                        />
                    </Grid>
                    <Grid item xs={12} sm={6} md={4}>
                        <SummaryCard
                            title="Tổng số phản hồi"
                            value={totalFeedbacks}
                            color={successColor}
                            IconComponent={FeedbackIcon}
                            onClick={handleNavigateFeedbacks}
                        />
                    </Grid>
                </Grid>

                <Divider sx={{ mb: 4 }} />

                {/* Doughnut Charts */}
                <Grid container spacing={4} sx={{ mb: 4 }}>
                    {/* Tình trạng tuyển khóa sinh */}
                    <Grid item xs={12} sm={6} md={4}>
                        <DoughnutChart
                            title="Tình trạng tuyển khóa sinh"
                            labels={["Đã duyệt", "Chưa duyệt"]}
                            data={[
                                totalRegisteredStudents > 0 ? totalStudents : 0,
                                totalRegisteredStudents > 0 ? totalRegisteredStudents - totalStudents : 0,
                            ]}
                            backgroundColors={[theme.palette.success.light, theme.palette.warning.light]}
                        />
                    </Grid>

                    {/* Tình trạng tuyển tình nguyện viên */}
                    <Grid item xs={12} sm={6} md={4}>
                        <DoughnutChart
                            title="Tình trạng tuyển tình nguyện viên"
                            labels={["Đã duyệt", "Chưa duyệt"]}
                            data={[
                                totalRegisteredVolunteers > 0 ? totalVolunteers : 0,
                                totalRegisteredVolunteers > 0 ? totalRegisteredVolunteers - totalVolunteers : 0,
                            ]}
                            backgroundColors={[theme.palette.success.light, theme.palette.warning.light]}
                        />
                    </Grid>

                    {/* Tổng kết điểm danh */}
                    <Grid item xs={12} sm={6} md={4}>
                        <DoughnutChart
                            title="Tổng kết điểm danh"
                            labels={["Có mặt", "Vắng"]}
                            data={[
                                Math.round(attendanceRate), // Chỉ lấy phần nguyên
                                100 - Math.round(attendanceRate),
                            ]}
                            backgroundColors={[theme.palette.primary.light, theme.palette.grey[300]]}
                            showPercentage={true} // Thêm prop này để hiển thị %
                        />
                    </Grid>
                    {/* Tỉ lệ giới tính khóa sinh */}
                    <Grid item xs={12} sm={6} md={4}>
                        <DoughnutChart
                            title="Tỉ lệ giới tính khóa sinh"
                            labels={["Nam", "Nữ"]}
                            data={[
                                totalStudents > 0 ? totalMaleStudents : 0,
                                totalStudents > 0 ? totalStudents - totalMaleStudents : 0,
                            ]}
                            backgroundColors={[theme.palette.info.light, theme.palette.error.light]}
                        />
                    </Grid>

                    {/* Tỉ lệ giới tính tình nguyện viên */}
                    <Grid item xs={12} sm={6} md={4}>
                        <DoughnutChart
                            title="Tỉ lệ giới tính tình nguyện viên"
                            labels={["Nam", "Nữ"]}
                            data={[
                                totalVolunteers > 0 ? totalMaleVolunteers : 0,
                                totalVolunteers > 0 ? totalVolunteers - totalMaleVolunteers : 0,
                            ]}
                            backgroundColors={[theme.palette.info.light, theme.palette.error.light]}
                        />
                    </Grid>

                    {/* Tỉ lệ tốt nghiệp */}
                    <Grid item xs={12} sm={6} md={4}>
                        <DoughnutChart
                            title="Tỉ lệ tốt nghiệp"
                            labels={["Đã tốt nghiệp", "Chưa tốt nghiệp"]}
                            data={[
                                Math.round(graduationRate), // Chỉ lấy phần nguyên
                                100 - Math.round(graduationRate),
                            ]}
                            backgroundColors={[theme.palette.success.light, theme.palette.error.light]}
                            showPercentage={true} // Thêm prop này để hiển thị %
                        />
                    </Grid>
                </Grid>
                {/* Registration Per Course Charts */}
                <Grid container spacing={4}>
                    {/* Student Registrations Per Course */}
                    <Grid item xs={12} md={6}>
                        <Paper sx={{ padding: 3, borderRadius: "16px" }}>
                            <Box
                                sx={{
                                    display: "flex",
                                    justifyContent: "space-between",
                                    alignItems: "center",
                                    mb: 2,
                                }}
                            >
                                <Typography variant="h6" color="primary">
                                    Số lượng học sinh đăng ký
                                </Typography>
                                <FormControl variant="outlined" size="small">
                                    <Select
                                        labelId="student-time-range-label"
                                        id="student-time-range"
                                        value={studentTimeRange}
                                        onChange={(e) => setStudentTimeRange(Number(e.target.value))}
                                        label="Khoảng thời gian"
                                    >
                                        {timeRanges.map((range) => (
                                            <MenuItem key={range.value} value={range.value}>
                                                {range.label}
                                            </MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            </Box>
                            {sortedStudentRegistrations && sortedStudentRegistrations.length > 0 ? (
                                <Bar
                                    data={{
                                        labels: sortedStudentRegistrations.map((d) => d.courseName),
                                        datasets: [
                                            {
                                                label: "Số lượng",
                                                data: sortedStudentRegistrations.map((d) => d.registrationCount),
                                                backgroundColor: theme.palette.primary.light,
                                            },
                                        ],
                                    }}
                                    options={{
                                        indexAxis: "y" as const, // Chuyển đổi biểu đồ cột ngang
                                        responsive: true,
                                        plugins: {
                                            legend: { display: false },
                                            tooltip: {
                                                callbacks: {
                                                    label: (tooltipItem: any) => `Số lượng: ${tooltipItem.parsed.x}`,
                                                    title: (tooltipItems: any) => tooltipItems[0].label, // Hiển thị đầy đủ tên khóa tu
                                                },
                                            },
                                        },
                                        scales: {
                                            x: {
                                                title: { display: true, text: "Số lượng" },
                                                beginAtZero: true,
                                            },
                                            // y: {
                                            //     title: { display: true, text: "Khóa tu" },
                                            //     ticks: {
                                            //         autoSkip: false,
                                            //         // Không cắt ngắn nhãn
                                            //     },
                                            // },
                                        },
                                    }}
                                />
                            ) : (
                                <Typography variant="body1">Không có dữ liệu</Typography>
                            )}
                        </Paper>
                    </Grid>

                    {/* Volunteer Registrations Per Course */}
                    <Grid item xs={12} md={6}>
                        <Paper sx={{ padding: 3, borderRadius: "16px" }}>
                            <Box
                                sx={{
                                    display: "flex",
                                    justifyContent: "space-between",
                                    alignItems: "center",
                                    mb: 2,
                                }}
                            >
                                <Typography variant="h6" color="primary">
                                    Số lượng tình nguyện viên đăng ký
                                </Typography>
                                <FormControl variant="outlined" size="small">
                                    <Select
                                        labelId="volunteer-time-range-label"
                                        id="volunteer-time-range"
                                        value={volunteerTimeRange}
                                        onChange={(e) => setVolunteerTimeRange(Number(e.target.value))}
                                        label="Khoảng thời gian"
                                    >
                                        {timeRanges.map((range) => (
                                            <MenuItem key={range.value} value={range.value}>
                                                {range.label}
                                            </MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                            </Box>
                            {sortedVolunteerRegistrations && sortedVolunteerRegistrations.length > 0 ? (
                                <Bar
                                    data={{
                                        labels: sortedVolunteerRegistrations.map((d) => d.courseName),
                                        datasets: [
                                            {
                                                label: "Số lượng",
                                                data: sortedVolunteerRegistrations.map((d) => d.registrationCount),
                                                backgroundColor: theme.palette.info.light,
                                            },
                                        ],
                                    }}
                                    options={{
                                        indexAxis: "y" as const, // Chuyển đổi biểu đồ cột ngang
                                        responsive: true,
                                        plugins: {
                                            legend: { display: false },
                                            tooltip: {
                                                callbacks: {
                                                    label: (tooltipItem: any) => `Số lượng: ${tooltipItem.parsed.x}`,
                                                    title: (tooltipItems: any) => tooltipItems[0].label, // Hiển thị đầy đủ tên khóa tu
                                                },
                                            },
                                        },
                                        scales: {
                                            x: {
                                                title: { display: true, text: "Số lượng" },
                                                beginAtZero: true,
                                            },
                                            // y: {
                                            //     title: { display: true, text: "Khóa tu" },
                                            //     ticks: {
                                            //         autoSkip: false,
                                            //         // Không cắt ngắn nhãn
                                            //     },
                                            // },
                                        },
                                    }}
                                />
                            ) : (
                                <Typography variant="body1">Không có dữ liệu</Typography>
                            )}
                        </Paper>
                    </Grid>
                </Grid>

                <Divider sx={{ mb: 4 }} />
            </>
        )
    }, [
        isLoading,
        isStudentLoading,
        isVolunteerLoading,
        error,
        studentError,
        volunteerError,
        totalStudents,
        totalVolunteers,
        totalFeedbacks,
        primaryColor,
        infoColor,
        successColor,
        attendanceRate,
        graduationRate,
        totalRegisteredStudents,
        totalRegisteredVolunteers,
        totalMaleStudents,
        totalMaleVolunteers,
        sortedStudentRegistrations,
        sortedVolunteerRegistrations,
        theme.palette.success.light,
        theme.palette.warning.light,
        theme.palette.primary.light,
        theme.palette.grey[300],
        theme.palette.info.light,
        theme.palette.error.light,
    ])

    return (
        <Box
            sx={{
                padding: { xs: 2, sm: 3 },
                backgroundColor: theme.palette.background.default,
                minHeight: "100vh",
            }}
        >
            {/* Header Section with Title and Course Selector */}
            <Box
                sx={{
                    display: "flex",
                    flexDirection: isSmallScreen ? "column" : "row",
                    alignItems: isSmallScreen ? "stretch" : "center",
                    justifyContent: "space-between",
                    mb: 4,
                }}
            >
                <Box className="mt-0 mb-2">
                    <Typography variant="h4" sx={{ fontWeight: "bold", color: theme.palette.primary.main }}>
                        <h2 className="fw-bold primary-color">Dashboard</h2>
                    </Typography>
                </Box>

                {/* Course Selection Dropdown */}
                <FormControl
                    variant="outlined"
                    sx={{
                        minWidth: 200,
                        maxWidth: 300,
                    }}
                >
                    <InputLabel id="course-select-label">Chọn khóa tu</InputLabel>
                    <Select
                        labelId="course-select-label"
                        id="course-select"
                        value={selectedCourseId || ""}
                        onChange={handleCourseChange}
                        label="Chọn khóa tu"
                    >
                        {courseOptions.map((course) => (
                            <MenuItem key={course.id} value={course.id}>
                                {course.name}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
            </Box>

            {/* Content Section */}
            {content}
        </Box>
    )
}

export default CourseDashboard

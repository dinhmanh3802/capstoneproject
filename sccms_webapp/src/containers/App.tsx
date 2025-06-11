import { useEffect } from "react"
import "../index.css"
import "../assets/css/style.css"
import { Outlet, useLocation, useNavigate } from "react-router-dom"
import { useDispatch, useSelector } from "react-redux"
import { setCredentials, logout } from "../store/slice/authSlice"
import { RootState } from "../store/store"
import { authModel } from "../interfaces"
import { Footer, Header, Navbar } from "../components/Layout"
import { deleteCurrentCourse, setCourses, setCurrentCourse } from "../store/slice/courseSlice"
import { useGetCourseQuery, useGetCurrentCourseQuery } from "../apis/courseApi"
import { MainLoader } from "../components/Page"
import jwtDecode from "jwt-decode"

function App() {
    const location = useLocation()
    const navigate = useNavigate()
    const dispatch = useDispatch()

    const auth = useSelector((state: RootState) => state.auth)
    const userData = auth.user

    // Sử dụng RTK Query để lấy courseData và currentCourseData
    const { data: courseData, isLoading: isCourseLoading } = useGetCourseQuery({})
    const { data: currentCourseData, isLoading: isCurrentCourseLoading } = useGetCurrentCourseQuery({})

    // Xử lý các course data
    useEffect(() => {
        if (!isCourseLoading && courseData) {
            dispatch(setCourses(courseData?.result))
        }
    }, [courseData, dispatch, isCourseLoading])

    // Xử lý current course data
    useEffect(() => {
        if (!isCurrentCourseLoading && currentCourseData) {
            dispatch(setCurrentCourse(currentCourseData?.result))
        }
    }, [currentCourseData, dispatch, isCurrentCourseLoading])

    // Xử lý authentication
    useEffect(() => {
        const localToken = auth.token

        if (localToken) {
            if (!userData) {
                try {
                    // Decode token để lấy thông tin user
                    const decoded: any = jwtDecode(localToken)
                    const { userId, username, role } = decoded
                    const user: authModel = { userId, username, role }
                    dispatch(setCredentials({ token: localToken, user }))
                } catch (error) {
                    // Nếu token không hợp lệ, xóa token và đăng xuất
                    dispatch(logout())
                    navigate("/home")
                }
            }
        } else {
            if (!userData?.userId) {
                navigate("/home")
            }
        }
    }, [auth.token, userData, dispatch, navigate])

    // Nếu đang tải dữ liệu course
    if (isCourseLoading || isCurrentCourseLoading) {
        return <MainLoader />
    }

    return (
        <div>
            {/* Header và Navbar */}
            <Header />
            {location.pathname !== "/home" && <Navbar />}
            <main id="main" className="main">
                {/* Outlet hiển thị các route con */}
                <Outlet />
            </main>
            <Footer />
        </div>
    )
}

export default App

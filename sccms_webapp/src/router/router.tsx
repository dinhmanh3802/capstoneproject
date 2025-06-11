import { createBrowserRouter } from "react-router-dom"
// import App from "../containers/App"
import {
    Login,
    CreateCourse,
    CourseDetail,
    ListCourse,
    ChangePassword,
    UserProfile,
    NotFound,
    CreateUser,
    UserList,
    StudentApplicationDetail,
    ListStudentGroup,
    UserDetail,
    ListStudent,
    StudentDetail,
    PickFreeTime,
    NightShiftConfig,
    MyNightShift,
    RejectedNightShiftAssignments,
    FreeTimeManager,
    ListVolunteerApplication,
    ListVolunteer,
    VolunteerDetail,
    StudentGroupDetail,
    CourseDashboard,
} from "../pages"
import ListUser from "../pages/user/ListUser"
import ForgotPassword from "../pages/auth/ForgotPassword"
import AuthLayout from "../components/Layout/AuthLayout"
import ListStudentApplication from "../pages/studentApplication/ListStudentApplication"
import { AdminRoute, LoginRoute, ManagerRoute, SecretaryRoute, StaffRoute, SupervisorRoute } from "../hoc"

//import HomePage from "../containers/HomePage"
import React, { Suspense } from "react"
import { CategoryPage, FeedbackSuccess, MainPage, StudentRegistration, SubmitSucess } from "../pages/guest"
import { SinglePage } from "../components/Page/guest"
import VolunteerRegistration from "../pages/guest/VolunteerRegistration"
import StudentResult from "../pages/guest/StudentResult"
import PostList from "../pages/post/PostList"
import Feedback from "../pages/guest/Feedback"
import PostDetail from "../pages/post/PostDetail"
import CreatePost from "../pages/post/CreatePost"
import SupervisorDetail from "../pages/supervisor/SupervisorDetail"
import { ListSupervisor, MainLoader } from "../components/Page"
import NightShiftManager from "../pages/nightShift/NightShiftManager"
import ListFeedback from "../pages/feedback/ListFeedback"
import VolunteerApplicationDetail from "../pages/volunteerApplication/VolunteerApplicationDetail"
import ListTeam from "../pages/team/ListTeam"
import TeamDetail from "../pages/team/TeamDetail"
import AttendanceReportsByDate from "../components/Page/dailyReport/manager/AttendanceReportsByDate"
import ReportDetail from "../components/Page/supervisor/ReportDetail"
import ErrorFallback from "../pages/common/ErrorFallback"

const HomePage = React.lazy(() => import("../containers/HomePage"))
const App = React.lazy(() => import("../containers/App"))

export const router = createBrowserRouter([
    {
        path: "/",
        element: (
            <Suspense fallback={<MainLoader></MainLoader>}>
                <App />
            </Suspense>
        ),
        errorElement: <ErrorFallback />,
        children: [
            { index: true, element: <CourseDashboard /> },
            { path: "/course", element: <ListCourse /> },
            { path: "/create-course", element: <CreateCourse /> },
            { path: "/change-password", element: <ChangePassword /> },
            { path: "/profile", element: <UserProfile /> },
            {
                path: "/user-list",
                element: (
                    <ManagerRoute>
                        <ListUser />
                    </ManagerRoute>
                ),
            },
            {
                path: "/user/:id",
                element: (
                    <SecretaryRoute>
                        <UserDetail />
                    </SecretaryRoute>
                ),
            },

            {
                path: "/supervisor-list",
                element: (
                    <ManagerRoute>
                        <ListSupervisor />
                    </ManagerRoute>
                ),
            },
            {
                path: "/supervisor/:id",
                element: (
                    <ManagerRoute>
                        <SupervisorDetail />
                    </ManagerRoute>
                ),
            },

            {
                path: "student-applications",
                element: (
                    <SecretaryRoute>
                        <ListStudentApplication />
                    </SecretaryRoute>
                ),
            },
            {
                path: "student-applications/:id",
                element: (
                    <SecretaryRoute>
                        <StudentApplicationDetail />
                    </SecretaryRoute>
                ),
            },
            {
                path: "students",
                element: (
                    <LoginRoute>
                        <ListStudent />
                    </LoginRoute>
                ),
            },
            {
                path: "students/:id/course/:courseId",
                element: (
                    <LoginRoute>
                        <StudentDetail />
                    </LoginRoute>
                ),
            },
            {
                path: "course/:id",
                element: <CourseDetail />,
            },
            {
                path: "course/create-course",
                element: <CreateCourse />,
            },
            {
                path: "student-groups",
                element: (
                    <LoginRoute>
                        <ListStudentGroup />
                    </LoginRoute>
                ),
            },
            {
                path: "student-groups/:id",
                element: (
                    <LoginRoute>
                        <StudentGroupDetail />
                    </LoginRoute>
                ),
            },
            {
                path: "/post",
                element: (
                    <ManagerRoute>
                        <PostList />
                    </ManagerRoute>
                ),
            },
            {
                path: "/post/create",
                element: (
                    <ManagerRoute>
                        <CreatePost />
                    </ManagerRoute>
                ),
            },
            {
                path: "/post/:id",
                element: (
                    <ManagerRoute>
                        <PostDetail />
                    </ManagerRoute>
                ),
            },
            {
                path: "/feedback",
                element: (
                    <ManagerRoute>
                        <ListFeedback />
                    </ManagerRoute>
                ),
            },
            {
                path: "volunteer-applications",
                element: (
                    <SecretaryRoute>
                        <ListVolunteerApplication />
                    </SecretaryRoute>
                ),
            },
            {
                path: "volunteers",
                element: (
                    <LoginRoute>
                        <ListVolunteer />
                    </LoginRoute>
                ),
            },
            {
                path: "volunteer/:volunteerId/course/:courseId",
                element: (
                    <LoginRoute>
                        <VolunteerDetail />
                    </LoginRoute>
                ),
            },
            {
                path: "volunteer-applications/:id",
                element: (
                    <SecretaryRoute>
                        <VolunteerApplicationDetail />
                    </SecretaryRoute>
                ),
            },
            {
                path: "team",
                element: (
                    <LoginRoute>
                        <ListTeam />
                    </LoginRoute>
                ),
            },
            {
                path: "team/:teamId",
                element: (
                    <LoginRoute>
                        <TeamDetail />
                    </LoginRoute>
                ),
            },
            {
                path: "free-time",
                element: (
                    <LoginRoute>
                        <PickFreeTime />
                    </LoginRoute>
                ),
            },
            {
                path: "free-time-manager",
                element: (
                    <SecretaryRoute>
                        <FreeTimeManager />
                    </SecretaryRoute>
                ),
            },
            {
                path: "night-shift-config",
                element: (
                    <SecretaryRoute>
                        <NightShiftConfig />
                    </SecretaryRoute>
                ),
            },
            {
                path: "my-night-shift",
                element: (
                    <LoginRoute>
                        <MyNightShift />
                    </LoginRoute>
                ),
            },
            {
                path: "night-shift-manager",
                element: (
                    <LoginRoute>
                        <NightShiftManager />
                    </LoginRoute>
                ),
            },
            {
                path: "reject-night-shift/:id?",
                element: (
                    <SecretaryRoute>
                        <RejectedNightShiftAssignments />
                    </SecretaryRoute>
                ),
            },

            {
                path: "report/:reportId",
                element: (
                    <LoginRoute>
                        <ReportDetail />
                    </LoginRoute>
                ),
            },

            {
                path: "attendance-reports",
                element: (
                    <LoginRoute>
                        <AttendanceReportsByDate />
                    </LoginRoute>
                ),
            },
        ],
    },
    {
        path: "/home",
        element: (
            <Suspense fallback={<MainLoader></MainLoader>}>
                <HomePage />
            </Suspense>
        ),
        errorElement: <ErrorFallback />,
        children: [
            { index: true, element: <MainPage /> },
            { path: "register/student", element: <StudentRegistration /> },
            { path: "register/volunteer", element: <VolunteerRegistration /> },
            { path: "category/:categoryID", element: <CategoryPage /> },
            { path: "post/:postId", element: <SinglePage /> },
            { path: "result", element: <StudentResult /> },
            { path: "feedback", element: <Feedback /> },
            { path: "submitsuccess", element: <SubmitSucess /> },
            { path: "feedbacksuccess", element: <FeedbackSuccess /> },
        ],
    },

    { path: "*", element: <NotFound /> },
    {
        path: "/auth",
        element: <AuthLayout />, // Chuyển đến AuthLayout cho các route liên quan đến đăng nhập
        children: [
            { path: "login", element: <Login /> },
            { path: "forgot-password", element: <ForgotPassword /> },
        ],
    },
])

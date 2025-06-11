// src/store/rootReducer.ts
import { combineReducers } from "@reduxjs/toolkit"
import authReducer, { logout } from "./slice/authSlice" // Sửa lại import
import { courseReducer } from "./slice/courseSlice"
import { userReducer } from "./slice/userSlice"
import authApi from "../apis/AuthApi"
import courseApi from "../apis/courseApi"
import userApi from "../apis/userApi"
import studentApplicationApi from "../apis/studentApplicationApi"
import supervisorApi from "../apis/supervisorApi"
import { supervisorReducer } from "./slice/supervisorSlice"
import { studentGroupApi } from "../apis/studentGroupApi"
import { emailTemplateApi } from "../apis/emailTemplateApi"
import { emailApi } from "../apis/emailApi"
import studentApi from "../apis/studentApi"
import { postReducer } from "./slice/postSlice"
import postApi from "../apis/postApi"
import notificationApi from "../apis/notificationApi"
import roomApi from "../apis/roomApi"
import nightShiftApi from "../apis/nightShiftApi"
import nightShiftAssignmentApi from "../apis/nightShiftAssignmentApi"
import staffFreeTimeApi from "../apis/staffFreeTimeApi"
import feedbackApi from "../apis/feedbackApi"
import volunteerApi from "../apis/volunteerApi"
import volunteerApplicationApi from "../apis/volunteerApplicationApi"
import teamApi from "../apis/teamApi"
import reportApi from "../apis/reportApi"

const appReducer = combineReducers({
    auth: authReducer, // Đổi từ userAuthStore thành auth
    [authApi.reducerPath]: authApi.reducer,
    courseStore: courseReducer,
    [courseApi.reducerPath]: courseApi.reducer,
    [studentApplicationApi.reducerPath]: studentApplicationApi.reducer,
    userStore: userReducer,
    [userApi.reducerPath]: userApi.reducer,
    supervisorStore: supervisorReducer,
    [supervisorApi.reducerPath]: supervisorApi.reducer,
    [studentGroupApi.reducerPath]: studentGroupApi.reducer,
    [emailTemplateApi.reducerPath]: emailTemplateApi.reducer,
    [emailApi.reducerPath]: emailApi.reducer,
    [studentApi.reducerPath]: studentApi.reducer,
    postStore: postReducer,
    [postApi.reducerPath]: postApi.reducer,
    studentStore: studentApi.reducer,
    [notificationApi.reducerPath]: notificationApi.reducer,
    [roomApi.reducerPath]: roomApi.reducer,
    [nightShiftApi.reducerPath]: nightShiftApi.reducer,
    [nightShiftAssignmentApi.reducerPath]: nightShiftAssignmentApi.reducer,
    [staffFreeTimeApi.reducerPath]: staffFreeTimeApi.reducer,
    [feedbackApi.reducerPath]: feedbackApi.reducer,
    [volunteerApi.reducerPath]: volunteerApi.reducer,
    [volunteerApplicationApi.reducerPath]: volunteerApplicationApi.reducer,
    [teamApi.reducerPath]: teamApi.reducer,
    [reportApi.reducerPath]: reportApi.reducer,
})

const rootReducer = (state: ReturnType<typeof appReducer> | undefined, action: any) => {
    if (action.type === logout.type) {
        // Reset toàn bộ state, bao gồm RTK Query cache
        state = undefined
    }
    return appReducer(state, action)
}

export default rootReducer

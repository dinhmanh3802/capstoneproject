// src/store/store.ts
import { configureStore } from "@reduxjs/toolkit"
import rootReducer from "./rootReducer"
import courseApi from "../apis/courseApi"
import userApi from "../apis/userApi"
import studentApplicationApi from "../apis/studentApplicationApi"
import { studentGroupApi } from "../apis/studentGroupApi"
import { emailTemplateApi } from "../apis/emailTemplateApi"
import { emailApi } from "../apis/emailApi"
import authApi from "../apis/AuthApi"
import supervisorApi from "../apis/supervisorApi"
import { postReducer } from "./slice/postSlice"
import postApi from "../apis/postApi"
import studentApi from "../apis/studentApi"
import notificationApi from "../apis/notificationApi"
import roomApi from "../apis/roomApi"
import nightShiftApi from "../apis/nightShiftApi"
import nightShiftAssignmentApi from "../apis/nightShiftAssignmentApi"
import staffFreeTimeApi from "../apis/staffFreeTimeApi"
import feedbackApi from "../apis/feedbackApi"
import volunteerApplicationApi from "../apis/volunteerApplicationApi"
import teamApi from "../apis/teamApi"
import reportApi from "../apis/reportApi"
import volunteerApi from "../apis/volunteerApi"

const store = configureStore({
    reducer: rootReducer, // Sử dụng rootReducer
    middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware()
            .concat(courseApi.middleware)
            .concat(authApi.middleware)
            .concat(userApi.middleware)
            .concat(supervisorApi.middleware)
            .concat(studentApplicationApi.middleware)
            .concat(postApi.middleware)
            .concat(studentGroupApi.middleware)
            .concat(emailTemplateApi.middleware)
            .concat(emailApi.middleware)
            .concat(studentApi.middleware)
            .concat(notificationApi.middleware)
            .concat(reportApi.middleware)
            .concat(roomApi.middleware)
            .concat(nightShiftApi.middleware)
            .concat(nightShiftAssignmentApi.middleware)
            .concat(staffFreeTimeApi.middleware)
            .concat(feedbackApi.middleware)
            .concat(volunteerApplicationApi.middleware)
            .concat(volunteerApi.middleware)
            .concat(teamApi.middleware),
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch

export default store

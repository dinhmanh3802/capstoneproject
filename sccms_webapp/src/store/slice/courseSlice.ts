import { createSlice, current } from "@reduxjs/toolkit"
import courseModel from "./../../interfaces/courseModel"

interface CourseState {
    courses: courseModel[]
    currentCourse: courseModel | null
}
const initialState: CourseState = {
    courses: [],
    currentCourse: null,
}

const courseSlice = createSlice({
    name: "course",
    initialState,
    reducers: {
        setCourses: (state, action) => {
            state.courses = action.payload
        },
        setCurrentCourse: (state, action) => {
            state.currentCourse = action.payload
        },
        deleteCurrentCourse: (state) => {
            state.currentCourse = null
        },
    },
})

export const { setCourses, setCurrentCourse, deleteCurrentCourse } = courseSlice.actions
export const courseReducer = courseSlice.reducer

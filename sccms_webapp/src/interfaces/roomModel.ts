import { SD_Gender } from "../utility/SD"
import studentGroupModel from "./studentGroupModel"

export interface roomModel {
    id: number
    courseId: number
    courseName: string
    name: string
    gender: SD_Gender
    numberOfStaff: number
    studentGroups: studentGroupModel[]
}

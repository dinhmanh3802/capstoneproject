import { SD_Gender, SD_Status } from "../utility/SD"
import studentModel from "./studentModel"
import { userModel } from "./userModel"

export default interface studentGroupModel {
    id: number
    courseId: number
    courseName: string
    groupName: string
    gender: SD_Gender
    status: SD_Status
    students: studentModel[]
    supervisors: userModel[]
    reports: any[]
}

import { SD_Gender, SD_Status } from "../utility/SD"
import volunteerModel from "./studentModel"
import { userModel } from "./userModel"

export default interface teamModel {
    id: number
    courseId: number
    courseName: string
    teamName: string
    gender: SD_Gender
    status: SD_Status
    volunteers: volunteerModel[]
    leaderId: number
    leader: userModel
    reports: any[]
    expectedVolunteers: number
}

import { SD_Gender, SD_ProcessStatus } from "../utility/SD"
import teamModel from "./teamModel"

export default interface volunteerModel {
    id?: number
    fullName?: string
    dateOfBirth?: Date
    gender?: SD_Gender
    image?: string
    nationalId?: string
    nationalImageFront?: string
    nationalImageBack?: string
    address?: string
    phoneNumber?: string
    email?: string
    status?: SD_ProcessStatus
    note?: string
    teams: teamModel
    volunteerCourses?: string[]
    VolunteerTeam?: string[]
}

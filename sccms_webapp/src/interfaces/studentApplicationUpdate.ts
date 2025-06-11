import { SD_ProcessStatus } from "../utility/SD"
export default interface studentApplicationUpdate {
    Ids?: number[]
    Status?: SD_ProcessStatus
    ReviewerId?: number
}

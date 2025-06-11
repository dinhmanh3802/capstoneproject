import { SD_PostStatus, SD_PostType } from "../utility/SD"

export interface postModel {
    id: number
    title: string
    content: string
    postDate: string
    image: string
    postType: SD_PostType
    status: SD_PostStatus
    userCreated: number
    userUpdated: number
    dateCreated: string
    dateModified: string
}

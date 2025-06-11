import { createSlice, PayloadAction } from "@reduxjs/toolkit"
import { postModel } from "../../interfaces/postModel"

interface PostState {
    posts: postModel[]
}

const initialState: PostState = {
    posts: [],
}

const postSlice = createSlice({
    name: "post",
    initialState,
    reducers: {
        getPosts: (state, action) => {
            state.posts = action.payload
        },
    },
})

export const { getPosts } = postSlice.actions
export const postReducer = postSlice.reducer

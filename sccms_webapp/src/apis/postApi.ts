import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { SD_BASE_URL } from "../utility/SD"

export const postApi = createApi({
    reducerPath: "postApi",
    baseQuery: fetchBaseQuery({ baseUrl: `${SD_BASE_URL}` }),
    tagTypes: ["Post"],
    endpoints: (builder) => ({
        // Get all posts with optional filters
        getPosts: builder.query({
            query: ({
                title,
                content,
                postDateStart,
                postDateEnd,
                status,
                postType,
                createdBy,
                pageNumber,
                pageSize,
            }) => ({
                url: "post",
                params: {
                    ...(title && { title }),
                    ...(content && { content }),
                    ...(postDateStart && { postDateStart: postDateStart }),
                    ...(postDateEnd && { postDateEnd: postDateEnd }),
                    ...(status !== undefined && { status }),
                    ...(postType !== undefined && { postType }),
                    ...(createdBy && { createdBy }),
                    ...(pageNumber && { pageNumber }),
                    ...(pageSize && { pageSize }),
                },
            }),
            providesTags: ["Post"],
        }),

        // Get a post by ID
        getPostById: builder.query({
            query: (id) => ({
                url: `post/${id}`,
            }),
            providesTags: (id) => [{ type: "Post", id }],
        }),

        // Create a new post
        createPost: builder.mutation({
            query: (postCreateDto) => ({
                url: "post",
                method: "POST",
                body: postCreateDto,
            }),
            invalidatesTags: ["Post"],
        }),

        // Update an existing post
        updatePost: builder.mutation({
            query: ({ id, postUpdateDto }) => {
                return {
                    url: `post/${id}`,
                    method: "PUT",
                    body: postUpdateDto,
                }
            },
            invalidatesTags: ({ id }) => [{ type: "Post", id: id }],
        }),

        // Delete a post by ID
        deletePost: builder.mutation({
            query: (id) => ({
                url: `post/${id}`,
                method: "DELETE",
            }),
            invalidatesTags: ["Post"],
        }),
    }),
})

export const {
    useGetPostsQuery,
    useGetPostByIdQuery,
    useCreatePostMutation,
    useUpdatePostMutation,
    useDeletePostMutation,
} = postApi

export default postApi

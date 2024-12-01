import { configureStore } from '@reduxjs/toolkit'
import graphReducer from './graphState/graphSlice'
import activityReducer from './activity/activitySlice'
import statesReducer from './states/statesSlice'
import actionsTemplatesReducer from './actions/actionTemplatesSlice'
import gearReducer from './gear/gearSlice'
// ...

export const store = configureStore({
  reducer: {
    graph: graphReducer,
    states: statesReducer,
    actionTemplates: actionsTemplatesReducer,
    activity: activityReducer,
    gear: gearReducer,
  },
})

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>
// Inferred type: {posts: PostsState, comments: CommentsState, users: UsersState}
export type AppDispatch = typeof store.dispatch

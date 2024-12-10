import { createAsyncThunk } from '@reduxjs/toolkit'
import { PointInTime, RpgGraphState } from '../lib/rpg-api/types'
import {
  InitiateAction,
  InitiateActionRequest,
  ActivityResponse,
  SetState,
  SetStateRequest,
  SetTimeRequest,
  SetTimeResponse,
  StringResponse,
  OverridePropValue,
  OverridePropValueResponse,
  OverridePropValueRequest,
} from '../lib/rpg-api/server-types'
import { RootState } from './store'

export type ThunkStatus = 'idle' | 'loading' | 'loaded'

export const fetchGraphState = createAsyncThunk(
  'graphState/fetch',
  async (id: string): Promise<RpgGraphState | undefined> => {
    const response = await get(`Cyborgs/PlayerCharacter/${id}`)
    const res = (await response.json()) as StringResponse
    return res?.graphState
  }
)

export const initiateAction = createAsyncThunk(
  'Cyborgs/action/initiate',
  async (
    initiateAction: InitiateAction,
    thunkAPI
  ): Promise<ActivityResponse | undefined> => {
    const graph = (thunkAPI.getState() as RootState).graph.graphState
    const op: InitiateActionRequest = {
      graphState: graph!,
      op: initiateAction,
    }
    const response = await post('Cyborgs/action/initiate', op)
    const res = (await response.json()) as ActivityResponse
    return res
  }
)

//{system}/entities/props/override
export const overridePropValue = createAsyncThunk(
  'Cyborgs/entity/prop/override',
  async (
    op: OverridePropValue,
    thunkAPI
  ): Promise<OverridePropValueResponse | undefined> => {
    const graph = (thunkAPI.getState() as RootState).graph.graphState
    const request: OverridePropValueRequest = {
      graphState: graph!,
      op: op,
    }
    const response = await post('Cyborgs/entities/props/override', request)
    const res = (await response.json()) as OverridePropValueResponse

    console.log('overridePropValue.response', res)

    return res
  }
)

export const setGraphTime = createAsyncThunk(
  'Cyborgs/time',
  async (time: PointInTime, thunkAPI): Promise<SetTimeResponse | undefined> => {
    const graph = (thunkAPI.getState() as RootState).graph.graphState
    const setTimeRequest: SetTimeRequest = {
      graphState: graph!,
      op: time,
    }
    const response = await post('Cyborgs/time', setTimeRequest)
    const res = (await response.json()) as SetTimeResponse

    console.log('setTime.response', res)

    return res
  }
)

export const toggleState = createAsyncThunk(
  'Cyborgs/state/toggle',
  async (setState: SetState, thunkAPI): Promise<StringResponse | undefined> => {
    const graph = (thunkAPI.getState() as RootState).graph.graphState
    const setStateRequest: SetStateRequest = {
      graphState: graph!,
      op: setState,
    }

    const response = await post('Cyborgs/state', setStateRequest)
    const res = (await response.json()) as StringResponse
    return res
  }
)

export const get = async (path: string) => {
  const response = await fetch(`api/${path}`, {
    method: 'GET',
    headers: {
      Accept: 'application/json; charset=utf-8',
      'Content-Type': 'application/json; charset=utf-8',
    },
  })

  return response
}

export const post = async (path: string, body?: unknown) => {
  const response = await fetch(`api/${path}`, {
    method: 'POST',
    headers: {
      Accept: 'application/json; charset=utf-8',
      'Content-Type': 'application/json; charset=utf-8',
    },
    body: body ? JSON.stringify(body) : null,
  })

  return response
}

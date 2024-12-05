import { useState } from 'react'
import { ModSetDescription, State } from '../../lib/rpg-api/types'
import { Grid, GridItem, Heading, Code } from '@chakra-ui/react'
import {
  selectGraphState,
  selectPlayerCharacter,
} from '../../app/graphState/graphSelectors'
import { useSelector } from 'react-redux'
import { useAppDispatch } from '../../app/hooks'
import { toggleState } from '../../app/thunks'
import { getStateDescription } from '../../lib/rpg-api/fetcher'
import BoxButton, { BoxButtonState } from '../ui/box-button'
import {
  PiCheckCircleFill,
  PiCheckCircleLight,
  PiQuestionLight,
} from 'react-icons/pi'
import {
  HoverCardArrow,
  HoverCardContent,
  HoverCardRoot,
  HoverCardTrigger,
} from '../ui/hover-card'
import ReactJson from 'react-json-view'

export declare interface StateButtonProps {
  className?: string
  state: State
}

function StatePanel(props: StateButtonProps) {
  const { state } = props

  const dispatch = useAppDispatch()
  const graphState = useSelector(selectGraphState)
  const playerCharacter = useSelector(selectPlayerCharacter)

  const [loadingDescribe, setLoadingDescribe] = useState<boolean>(false)
  const [describe, setDescribe] = useState<
    ModSetDescription | undefined | null
  >()
  const [open, setOpen] = useState(false)

  const onChangeState = (buttonState: BoxButtonState) => {
    console.log('clicked', buttonState)
    if (playerCharacter) {
      console.log('playercharacter', playerCharacter)
      dispatch(
        toggleState({
          entityId: playerCharacter.id,
          state: state.name,
          on: !state.isOn,
        })
      )
    }
  }

  const onDescribe = async (open: boolean) => {
    if (state) {
      if (!describe && !loadingDescribe) {
        setLoadingDescribe(true)
        const response = await getStateDescription(
          state.ownerId,
          state.name,
          graphState!
        )
        setDescribe(response?.data)
        setLoadingDescribe(false)
      }
      setOpen(open)
    }
  }

  return (
    <>
      <BoxButton
        width={'100%'}
        state={state.isOn ? 'on' : 'off'}
        onClick={() => onChangeState('on')}
      >
        <Grid templateColumns="repeat(6, 1fr)" gap={4} width={'100%'}>
          <GridItem colSpan={5} h="10">
            <Heading as={'h3'} size={'sm'}>
              {state.name}
            </Heading>
          </GridItem>
          <GridItem colStart={6} h="10">
            {state.isOn ? (
              <PiCheckCircleFill size={'28px'} />
            ) : (
              <PiCheckCircleLight size={'28px'} />
            )}
          </GridItem>
        </Grid>
      </BoxButton>

      <HoverCardRoot
        size="sm"
        open={open}
        onOpenChange={async (e) => await onDescribe(e.open)}
      >
        <HoverCardTrigger asChild>
          <PiQuestionLight />
        </HoverCardTrigger>

        <HoverCardContent>
          <HoverCardArrow />
          {describe && <ReactJson src={describe} />}
        </HoverCardContent>
      </HoverCardRoot>
    </>
  )
}

export default StatePanel

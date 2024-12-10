import { State } from '@lib/rpg-api/types'
import { Grid, GridItem, Heading } from '@chakra-ui/react'
import { selectPlayerCharacter } from '@app/graphState/graphSelectors'
import { useSelector } from 'react-redux'
import { useAppDispatch } from '@app/hooks'
import { toggleState } from '@app/thunks'

import BoxButton, { BoxButtonState } from '@components/ui/box-button'
import { PiCheckCircleFill, PiCheckCircleLight } from 'react-icons/pi'

export declare interface StateButtonProps {
  className?: string
  state: State
}

function StatePanel(props: StateButtonProps) {
  const { state } = props

  const dispatch = useAppDispatch()
  const playerCharacter = useSelector(selectPlayerCharacter)

  const onChangeState = async (buttonState: BoxButtonState) => {
    console.log('clicked', buttonState)
    if (playerCharacter) {
      console.log('playercharacter', playerCharacter)
      await dispatch(
        toggleState({
          entityId: playerCharacter.id,
          state: state.name,
          on: !state.isOn,
        })
      )
    }
  }

  return (
    <>
      <BoxButton
        width={'100%'}
        state={state.isOn ? 'on' : 'off'}
        onClick={async () => await onChangeState('on')}
      >
        <Grid templateColumns="repeat(6, 1fr)" gap={4} width={'100%'}>
          <GridItem colSpan={5} h="10">
            <Heading as={'h3'} size={'sm'}>
              {state.name}
            </Heading>
          </GridItem>
          <GridItem colStart={6} h="10">
            {state.isOn ? (
              <PiCheckCircleFill size={'16px'} />
            ) : (
              <PiCheckCircleLight size={'16px'} />
            )}
          </GridItem>
        </Grid>
      </BoxButton>
    </>
  )
}

export default StatePanel

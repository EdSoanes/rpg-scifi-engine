
import React from 'react'
import { State } from '../../lib/rpg-api/types'
import { Button } from '@chakra-ui/react'
import { CheckCircleIcon, SmallCloseIcon } from '@chakra-ui/icons'
import { selectPlayerCharacter } from '../../app/graphState/graphSelectors'
import { useSelector } from 'react-redux'
import { useAppDispatch } from '../../app/hooks'
import { toggleState } from '../../app/thunks'

export declare interface StateButtonProps {
  state: State
}

function StateButton(props: StateButtonProps) {
  const dispatch = useAppDispatch()
  const playerCharacter = useSelector(selectPlayerCharacter)
  const variant = props.state.isOn ? 'solid' : 'outline'

  const onChangeState = async () => {
    if (playerCharacter) {
      dispatch(toggleState({
        entityId: playerCharacter.id,
        state: props.state.name,
        on: !props.state.isOn,
      }))
    }
  }

  return (
    <Button
      leftIcon={props.state.isOn ? <CheckCircleIcon /> : <SmallCloseIcon />}
      variant={variant}
      size={'lg'}
      onClick={onChangeState}
    >
      {props.state.name}
    </Button>
  )
}

export default StateButton

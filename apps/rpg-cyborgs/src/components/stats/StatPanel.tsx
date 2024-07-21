import { atom, Atom, useAtom } from 'jotai'
import {
  Stat,
  StatNumber,
  StatHelpText,
  StatLabel,
  StatArrow,
  IconButton,
  useDisclosure,
  Code,
  Button,
  Modal,
  ModalBody,
  ModalCloseButton,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
} from '@chakra-ui/react'
import React from 'react'
import { PropDesc, PropValue } from '../../lib/rpg-api/types'
import { QuestionOutlineIcon } from '@chakra-ui/icons'
import { getPropDesc } from '../../lib/rpg-api/fetcher'
import { graphStateAtom } from '../atoms/graphState.atom'

const describeAtom = atom<PropDesc | null>(null)

export declare interface StatPanelProps {
  propName: string
  propNameAbbr: string
  propValueAtom: Atom<PropValue | null>
}

function StatPanel(props: StatPanelProps) {
  const [graphState] = useAtom(graphStateAtom)
  const [propValue] = useAtom(props.propValueAtom)
  const [describe, setDescribe] = useAtom(describeAtom)

  const eq = (propValue?.value ?? 0) === (propValue?.baseValue ?? 0)
  const inc = !eq && (propValue?.value ?? 0) > (propValue?.baseValue ?? 0)
  const dec = !eq && (propValue?.value ?? 0) < (propValue?.baseValue ?? 0)

  const onDescribe = async () => {
    if (propValue?.entityPropRef) {
      const prop = `${propValue.entityPropRef.prop}.Value`
      const desc = await getPropDesc(
        propValue.entityPropRef.entityId,
        prop,
        graphState!
      )
      setDescribe(desc)
      onOpen()
    }
  }

  const { onOpen, onClose, isOpen } = useDisclosure()

  return (
    <>
      <Stat m={4} p={4} border="1px" borderRadius={4} borderColor={'lightgray'}>
        <StatLabel>{props.propNameAbbr}</StatLabel>
        <StatNumber>{propValue?.value ?? 0}</StatNumber>
        <StatHelpText>
          {inc && <StatArrow type="increase" />}
          {dec && <StatArrow type="decrease" />}
          {props.propName} {propValue?.baseValue ?? 0}
          <IconButton
            variant={'ghost'}
            aria-label="describe"
            size="lg"
            icon={<QuestionOutlineIcon />}
            onClick={onDescribe}
          />
        </StatHelpText>
      </Stat>
      <Modal isOpen={isOpen} onClose={onClose}>
        <ModalOverlay />
        <ModalContent>
          <ModalHeader>{describe?.rootProp ?? '-'}</ModalHeader>
          <ModalCloseButton />
          <ModalBody>
            <Code>{JSON.stringify(describe, null, 2)}</Code>
          </ModalBody>

          <ModalFooter>
            <Button colorScheme="blue" mr={3} onClick={onClose}>
              Close
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    </>
  )
}

export default StatPanel

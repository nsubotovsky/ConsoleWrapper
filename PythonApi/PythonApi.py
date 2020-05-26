import fileinput


class Py3Stdout( object ):

    def __call__( self, *args, **kwargs ):
        kwargs['flush'] = True
        kwargs['end'] = '\n'
        print( *args, **kwargs )


class Py3Stdin( object ):

    def __call__( self, *args, **kwargs ):
        return input()


class Algorithm( object ):

    _SHORTCUTS_PREFIX = 'NEXT> '

    def __init__( self, outputFunc ):
        self.outputFunc = outputFunc
    
    def shouldQuit( self, cmd ):
        raise NotImplementedError()

    def processMsg( self, cmd ):
        raise NotImplementedError()

    def getShortcuts( self ):
        raise NotImplementedError()

    def outputShortcuts( self ):
        if self.getShortcuts():
            self.outputFunc( self._SHORTCUTS_PREFIX + ' | '.join( '{}:{}'.format( key, value ) for key, value in self.getShortcuts().items() ) )




class Echo( Algorithm ):
    

    def shouldQuit( self, cmd ):
        if 'quit' in cmd.lower():
            self.outputFunc('Quitting...')
            return True

        return False

    def processMsg( self, cmd ):
        self.outputFunc( 'ECHO> {}'.format( cmd.strip( ' \n' ) ) )

    def getShortcuts( self ):
        return {'F1':'Do Something', 'F2':'Do Another thing'}


class Api( object ):

    def __init__( self, outputFunc=None, inputFunc=None, algorithm=None, echo='' ):

        self.outputFunc = outputFunc or Py3Stdout()
        self.inputFunc = inputFunc or Py3Stdin()
        self.echo = echo

        self.outputFunc('Initializing algorithm...')
        self.algorithm = algorithm or Echo( outputFunc=self.outputFunc )
        self.outputFunc('Api Ready')


    def go( self ):

        while True:

            cmd = self.inputFunc()

            if self.algorithm.shouldQuit( cmd ): return
                
            if self.echo: self.outputFunc( self.echo.format( cmd ) )

            self.algorithm.processMsg( cmd )
            self.algorithm.outputShortcuts()


Api( echo=':ExECHOx:{}' ).go()



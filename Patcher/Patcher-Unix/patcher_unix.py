
import click
import mmap

@click.command()
@click.option('-g', '--game', default=2, type=int, help='Borderlands version. "--game 1" for Borderlands 1, "--game 2" for Borderlands 2, "--game 3" for Borderlands TPS')
@click.option('consoleKey', '--console-key', default='Tilde', help='Keybind to open console.')
@click.option('--debug', is_flag=True, help='Enable debug mode. Do not do this unless you know what you are doing.')
@click.argument('path', default='', type=click.Path(exists=True))

def patch(game, consoleKey, debug, path):
    click.echo('Hello, world!')

if __name__ == '__main__':
    patch()